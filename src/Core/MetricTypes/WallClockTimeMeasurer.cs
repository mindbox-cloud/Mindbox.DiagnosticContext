#nullable disable

using System;

namespace Itc.Commons
{
	internal sealed class WallClockTimeMeasurer : MetricsMeasurer
	{
		private ReliableStopwatchWithCpuTime stopwatch = null;

		public WallClockTimeMeasurer(string metricsTypeSystemName) : base(metricsTypeSystemName)
		{
		}

		protected override long? GetValueCore()
		{
			return stopwatch.Elapsed.Ticks;
		}

		protected override void StartCore()
		{
			stopwatch = new ReliableStopwatchWithCpuTime(false);
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
