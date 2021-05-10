#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using Itc.Commons.Model;

namespace Itc.Commons
{
	internal sealed class CpuTimeMeasurer : MetricsMeasurer
	{
		private MetricsMeasurer innerMeasurer = null;

		public CpuTimeMeasurer(string metricsTypeSystemName) : base(metricsTypeSystemName)
		{
			
		}

		protected override long? GetValueCore()
		{
			return innerMeasurer.GetValue();
		}

		protected override void StartCore()
		{
			innerMeasurer = IsRoot 
				? (MetricsMeasurer)new ThreadAwareCpuLoadValueProvider() 
				: new CurrentThreadCpuLoadValueProvider();

			innerMeasurer.Start();
		}

		protected override void StopCore()
		{
			innerMeasurer.Stop();
		}

		private sealed class ThreadAwareCpuLoadValueProvider : MetricsMeasurer
		{
			private const long NotSetTicks = -1;

			private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
			private static readonly IntPtr NULL_HANDLE_VALUE = new IntPtr(0);
			private const int THREAD_QUERY_INFORMATION = 0x0040;

			private const int MissingThreadErrorCode = 87;

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern bool GetThreadTimes(
				IntPtr handle,
				out long creationTime,
				out long exitTime,
				out long kernelTime,
				out long userTime);

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern uint GetCurrentThreadId();

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern IntPtr OpenThread(
				int access,
				bool inherit,
				uint threadId);

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern bool CloseHandle(IntPtr handle);
			
			private readonly uint currentThreadId;

			private long startTicks = NotSetTicks;
			private long stopTicks = NotSetTicks;

			private long? threadCreationTime = null;

			private const int AccessDeniedErrorCode = 5;

			public ThreadAwareCpuLoadValueProvider() : base("InnerMetrics")
			{
				currentThreadId = GetCurrentThreadId();
			}

			protected override long? GetValueCore()
			{
				var localStartTicks = Volatile.Read(ref startTicks);
				if (localStartTicks == NotSetTicks)
					return null;

				var localStopTicks = Volatile.Read(ref stopTicks);
				var localRightBorderTicks = localStopTicks == NotSetTicks ? TryGetThreadCpuLoadTicks() : localStopTicks;
				if (!localRightBorderTicks.HasValue)
				{
					// there is a possibility of a race condition here
					localStopTicks = Volatile.Read(ref stopTicks);
					if (localStopTicks == NotSetTicks)
						return null;

					localRightBorderTicks = localStopTicks;
				}

				var result = localRightBorderTicks.Value - localStartTicks;

				return result;
			}

			protected override void StartCore()
			{
				Volatile.Write(ref startTicks, TryGetThreadCpuLoadTicks() ?? NotSetTicks);
			}

			protected override void StopCore()
			{
				var actualCurrentThread = GetCurrentThreadId();
				if (currentThreadId != actualCurrentThread)
				{
					ModelApplicationHostController.Instance.LogError(
						"Calling Stop in a different thread than Start call means that metrics doesn't work." +
						$"Start call ThreadId: {currentThreadId}, Stop call ThreadId: {actualCurrentThread}",
						logLevelOverride: ItcLogLevel.Error);

					Volatile.Write(ref startTicks, NotSetTicks);
					Volatile.Write(ref stopTicks, NotSetTicks);
					return;
				}

				Volatile.Write(ref stopTicks, TryGetThreadCpuLoadTicks() ?? NotSetTicks);
			}

			private long? TryGetThreadCpuLoadTicks()
			{
				IntPtr? threadHandle = null;
				try
				{
					threadHandle = OpenThread(THREAD_QUERY_INFORMATION, false, currentThreadId);
					if (threadHandle == INVALID_HANDLE_VALUE || threadHandle == NULL_HANDLE_VALUE)
					{
						var error = Marshal.GetLastWin32Error();

						// it means that this thread is not alive anymore
						if (error == MissingThreadErrorCode)
							return null;

						// means we couldn't get access to the thread, doesn't occur too often,
						// perhaps it means the thread is in accessible state (finishing or else)
						if (error == AccessDeniedErrorCode)
							return null;

						ModelApplicationHostController.Instance.LogError(
							$"Couldn't open thread with id {currentThreadId}, handle is invalid. Last error code is {error}",
							logLevelOverride: ItcLogLevel.Error);
						return null;
					}

					if (!GetThreadTimes(
						threadHandle.Value,
						out var creationTime,
						out var exitTime,
						out var kernelTime,
						out var userTime))
					{
						var error = Marshal.GetLastWin32Error();

						ModelApplicationHostController.Instance.LogError(
							$"Couldn't get thread cpu time for thread with id {currentThreadId} and handle {threadHandle}. " +
							$"Error code is {error}",
							logLevelOverride: ItcLogLevel.Error);
						return null;
					}

					if (threadCreationTime.HasValue && threadCreationTime.Value != creationTime)
					{
						ModelApplicationHostController.Instance.LogError(
							$"Thread {currentThreadId} had creationTime = {threadCreationTime.Value}, " +
							$"but now it's creationTime is {creationTime}",
							logLevelOverride: ItcLogLevel.Error);
						return null;
					}

					if (!threadCreationTime.HasValue)
						threadCreationTime = creationTime;

					return kernelTime + userTime;
				}
				finally
				{
					if (threadHandle.HasValue)
						CloseHandle(threadHandle.Value);
				}
			}
		}

		private sealed class CurrentThreadCpuLoadValueProvider : MetricsMeasurer
		{
			public CurrentThreadCpuLoadValueProvider() : base("InnerMetrics")
			{
			}

			private ReliableStopwatchWithCpuTime stopwatch = null;

			protected override long? GetValueCore()
			{
				return stopwatch.IsRunning ? 0 : stopwatch.ElapsedCpu.Ticks;
			}

			protected override void StartCore()
			{
				stopwatch = new ReliableStopwatchWithCpuTime(true);
				stopwatch.Start();
			}

			protected override void StopCore()
			{
				if (stopwatch == null)
					throw new InvalidOperationException("stopwatch == null");

				stopwatch.Stop();
			}
		}
	}
}
