#nullable disable

using System;

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	internal sealed class WallClockTimeMeasurer : MetricsMeasurer
	{
		private ReliableStopwatchWithCpuTime stopwatch = null;

		public WallClockTimeMeasurer(ICurrentTimeAccessor currentTimeAccessor, string metricsTypeSystemName) : base(currentTimeAccessor, metricsTypeSystemName)
		{
		}

		protected override long? GetValueCore()
		{
			return stopwatch.Elapsed.Ticks;
		}

		protected override void StartCore()
		{
			stopwatch = new ReliableStopwatchWithCpuTime(CurrentTimeAccessor,false);
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
