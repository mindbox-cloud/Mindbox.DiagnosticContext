#nullable disable

using System;

namespace Itc.Commons
{
	internal class WallClockTimeMetricsType : MetricsType<WallClockTimeMeasurer>
	{
		public static WallClockTimeMetricsType Create(string systemName)
		{
			return new WallClockTimeMetricsType(systemName);
		}

		public override string Units => "[ms]";

		public override long ConvertMetricValue(long rawMetricValue) =>
			(long)TimeSpan.FromTicks(rawMetricValue).TotalMilliseconds;

		private WallClockTimeMetricsType(string systemName) : base(systemName, NullMetricsMeasurerCreationHandler.Instance)
		{
		}

		protected override WallClockTimeMeasurer CreateMeasurerCore()
		{
			return new WallClockTimeMeasurer(SystemName);
		}
	}
}
