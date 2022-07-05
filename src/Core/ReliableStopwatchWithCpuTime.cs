// Copyright 2021 Mindbox Ltd
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#nullable disable

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mindbox.DiagnosticContext;

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
	private readonly ICurrentTimeAccessor _currentTimeAccessor;

	private static readonly Lazy<IntPtr> _currentThread = new(
		() =>
		{
			Thread.BeginThreadAffinity();
			return GetCurrentThread();
		});

	private TimeSpan TotalProcessorTime
	{
		get
		{
			GetThreadTimes(_currentThread.Value, out _, out _, out var kernelTime, out var userTime);
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


	private DateTime _lastStartTimeUtc;
	private TimeSpan _lastStopElapsed;
	private TimeSpan _lastStartCpuTime;
	private TimeSpan _lastStopElapsedCpu;


	public ReliableStopwatchWithCpuTime(ICurrentTimeAccessor currentTimeAccessor, bool enableCpuTime = false)
	{
		_currentTimeAccessor = currentTimeAccessor;
		IsCpuTimeEnabled = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && enableCpuTime;
	}


	public TimeSpan Elapsed
	{
		get { return IsRunning ? _lastStopElapsed + ElapsedSinceLastStart : _lastStopElapsed; }
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

			return IsRunning ? _lastStopElapsedCpu + ElapsedCpuSinceLastStart : _lastStopElapsedCpu;
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
			var result = _currentTimeAccessor.CurrentDateTimeUtc - _lastStartTimeUtc;
			return result < TimeSpan.Zero ? TimeSpan.Zero : result;
		}
	}

	private TimeSpan ElapsedCpuSinceLastStart
	{
		get
		{
			if (!IsCpuTimeEnabled)
				throw new InvalidOperationException("!isCpuTimeEnabled");
			var result = TotalProcessorTime - _lastStartCpuTime;
			return result < TimeSpan.Zero ? TimeSpan.Zero : result;
		}
	}


	public void Reset()
	{
		IsRunning = false;
		_lastStopElapsed = TimeSpan.Zero;
		_lastStopElapsedCpu = TimeSpan.Zero;
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
				_lastStartCpuTime = TotalProcessorTime;
			_lastStartTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc;
		}
	}

	public void Stop()
	{
		if (IsRunning)
		{
			if (IsCpuTimeEnabled)
				_lastStopElapsedCpu += ElapsedCpuSinceLastStart;
			_lastStopElapsed += ElapsedSinceLastStart;
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
		private readonly Action<ReliableStopwatchWithCpuTime> _resultProcessor;
		private ReliableStopwatchWithCpuTime _stopwatch;


		public Measurer(
			Action<ReliableStopwatchWithCpuTime> resultProcessor,
			ICurrentTimeAccessor currentTimeAccessor,
			bool enableCpuTime)
		{
			_resultProcessor = resultProcessor;
			if (resultProcessor == null)
				throw new ArgumentNullException(nameof(resultProcessor));

			_stopwatch = new ReliableStopwatchWithCpuTime(currentTimeAccessor, enableCpuTime);
			_stopwatch.Start();
		}

		public void Dispose()
		{
			if (_stopwatch != null)
			{
				_stopwatch.Stop();
				_resultProcessor(_stopwatch);
				_stopwatch = null;
			}
		}
	}
}