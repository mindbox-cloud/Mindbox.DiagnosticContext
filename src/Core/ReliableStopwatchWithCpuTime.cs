#nullable disable

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mindbox.DiagnosticContext
{
	/// <summary>
	/// Позволяет измерять, сколько реального и сколько процессорного времени было потрачено на выполнение участка кода.
	/// Если включено измерение процессорного времени, 
	/// код обязательно должен выполняться в одном потоке и в этом потоке не должно в промежутках выполняться ничего другого,
	/// поддерживаются только архитектуры, где логический поток не должен переключаться между разными физическими.
	/// Измерение не очень точное, но не становится некорректным ни при изменении тактовой частоты процессора,
	/// ни при выполнении на разных процессорах в системах с проблемными BIOS или HAL (в отличие от Stopwatch).
	/// </summary>
	/// <remarks>
	/// Не следует использовать, в короткоживущих потоках, которые создаются и потом уничтожаются,
	/// а потом создаются новые. На каждый поток выделяется некоторое количество ресурсов, которые никогда не освобождаются.
	/// </remarks>
	public class ReliableStopwatchWithCpuTime : IElapsedMillisecondsMeasurements
	{
		private readonly ICurrentTimeAccessor currentTimeAccessor;

		private static readonly Lazy<IntPtr> currentThread = new Lazy<IntPtr>(
			() =>
			{
				Thread.BeginThreadAffinity();
				return GetCurrentThread();
			});

		private TimeSpan TotalProcessorTime
		{
			get
			{
				GetThreadTimes(currentThread.Value, out _, out _, out var kernelTime, out var userTime);
				return new TimeSpan(kernelTime + userTime);
			}
		}


		public static IDisposable CreateMeasurer(
			Action<IElapsedMillisecondsMeasurements> resultProcessor,
			ICurrentTimeAccessor currentTimeAccessor,
			bool enableCpuTime = false)
		{
			if (resultProcessor == null)
				throw new ArgumentNullException(nameof(resultProcessor));

			return new Measurer(resultProcessor, currentTimeAccessor, enableCpuTime);
		}

		public static ReliableStopwatchWithCpuTime StartNew(ICurrentTimeAccessor currentTimeAccessor)
		{
			var result = new ReliableStopwatchWithCpuTime(currentTimeAccessor);
			result.Start();
			return result;
		}


		private DateTime lastStartTimeUtc;
		private TimeSpan lastStopElapsed;
		private TimeSpan lastStartCpuTime;
		private TimeSpan lastStopElapsedCpu;


		public ReliableStopwatchWithCpuTime(ICurrentTimeAccessor currentTimeAccessor, bool enableCpuTime = false)
		{
			this.currentTimeAccessor = currentTimeAccessor;
			IsCpuTimeEnabled = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && enableCpuTime;
		}


		public TimeSpan Elapsed
		{
			get { return IsRunning ? lastStopElapsed + ElapsedSinceLastStart : lastStopElapsed; }
		}

		public long ElapsedMilliseconds
		{
			get { return (long)Elapsed.TotalMilliseconds; }
		}

		public TimeSpan ElapsedCpu
		{
			get
			{
				if (!IsCpuTimeEnabled)
				{
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
						throw new InvalidOperationException("!isCpuTimeEnabled");

					return TimeSpan.Zero;
				}

				return IsRunning ? lastStopElapsedCpu + ElapsedCpuSinceLastStart : lastStopElapsedCpu;
			}
		}

		public long ElapsedCpuMilliseconds
		{
			get
			{
				return (long)ElapsedCpu.TotalMilliseconds;
			}
		}

		public bool IsRunning { get; private set; }
		public bool IsCpuTimeEnabled { get; private set; }


		private TimeSpan ElapsedSinceLastStart
		{
			get
			{
				var result = currentTimeAccessor.CurrentDateTimeUtc - lastStartTimeUtc;
				return result < TimeSpan.Zero ? TimeSpan.Zero : result;
			}
		}

		private TimeSpan ElapsedCpuSinceLastStart
		{
			get
			{
				if (!IsCpuTimeEnabled)
					throw new InvalidOperationException("!isCpuTimeEnabled");
				var result = TotalProcessorTime - lastStartCpuTime;
				return result < TimeSpan.Zero ? TimeSpan.Zero : result;
			}
		}


		public void Reset()
		{
			IsRunning = false;
			lastStopElapsed = TimeSpan.Zero;
			lastStopElapsedCpu = TimeSpan.Zero;
		}

		public void Restart()
		{
			Reset();
			Start();
		}

		public void Start()
		{
			if (!IsRunning)
			{
				IsRunning = true;
				if (IsCpuTimeEnabled)
					lastStartCpuTime = TotalProcessorTime;
				lastStartTimeUtc = currentTimeAccessor.CurrentDateTimeUtc;
			}
		}

		public void Stop()
		{
			if (IsRunning)
			{
				if (IsCpuTimeEnabled)
					lastStopElapsedCpu += ElapsedCpuSinceLastStart;
				lastStopElapsed += ElapsedSinceLastStart;
				IsRunning = false;
			}
		}

		[DllImport("kernel32.dll")]
		private static extern bool GetThreadTimes(
			IntPtr handle,
			out long creationTime,
			out long exitTime,
			out long kernelTime,
			out long userTime);

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetCurrentThread();

		private class Measurer : IDisposable
		{
			private readonly Action<ReliableStopwatchWithCpuTime> resultProcessor;
			private ReliableStopwatchWithCpuTime stopwatch;


			public Measurer(
				Action<ReliableStopwatchWithCpuTime> resultProcessor,
				ICurrentTimeAccessor currentTimeAccessor,
				bool enableCpuTime)
			{
				this.resultProcessor = resultProcessor;
				if (resultProcessor == null)
					throw new ArgumentNullException(nameof(resultProcessor));

				stopwatch = new ReliableStopwatchWithCpuTime(currentTimeAccessor, enableCpuTime);
				stopwatch.Start();
			}
			
			public void Dispose()
			{
				if (stopwatch != null)
				{
					stopwatch.Stop();
					resultProcessor(stopwatch);
					stopwatch = null;
				}
			}
		}
	}
}
