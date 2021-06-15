#nullable disable

using System;

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	internal class WallClockTimeMetricsType : MetricsType<WallClockTimeMeasurer>
	{
		public static WallClockTimeMetricsType Create(ICurrentTimeAccessor currentTimeAccessor, string systemName)
		{
			return new WallClockTimeMetricsType(currentTimeAccessor, systemName);
		}

		public override string Units => "[ms]";

		public override long ConvertMetricValue(long rawMetricValue) =>
			(long)TimeSpan.FromTicks(rawMetricValue).TotalMilliseconds;

		private WallClockTimeMetricsType(ICurrentTimeAccessor currentTimeAccessor, string systemName) : base(currentTimeAccessor, systemName, NullMetricsMeasurerCreationHandler.Instance)
		{
		}

		protected override WallClockTimeMeasurer CreateMeasurerCore()
		{
			return new WallClockTimeMeasurer(CurrentTimeAccessor, SystemName);
		}
	}
}
