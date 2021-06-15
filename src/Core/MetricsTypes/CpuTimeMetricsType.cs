#nullable disable

using System;

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	internal class CpuTimeMetricsType : MetricsType<CpuTimeMeasurer>
	{
		public static CpuTimeMetricsType Create(ICurrentTimeAccessor currentTimeAccessor, string systemName, IMetricsMeasurerCreationHandler handler)
		{
			return new CpuTimeMetricsType(currentTimeAccessor, systemName, handler);
		}

		public override string Units => "[ms]";

		public override long ConvertMetricValue(long rawMetricValue) =>
			(long)TimeSpan.FromTicks(rawMetricValue).TotalMilliseconds;

		private CpuTimeMetricsType(ICurrentTimeAccessor currentTimeAccessor, string systemName, IMetricsMeasurerCreationHandler handler) : base(currentTimeAccessor, systemName, handler)
		{
		}

		protected override CpuTimeMeasurer CreateMeasurerCore()
		{
			return new CpuTimeMeasurer(CurrentTimeAccessor, SystemName);
		}
	}
}
