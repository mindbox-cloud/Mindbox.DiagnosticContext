#nullable disable

using System;

namespace Itc.Commons
{
	internal class CpuTimeMetricsType : MetricsType<CpuTimeMeasurer>
	{
		public static CpuTimeMetricsType Create(string systemName, IMetricsMeasurerCreationHandler handler)
		{
			return new CpuTimeMetricsType(systemName, handler);
		}

		public override string Units => "[ms]";

		public override long ConvertMetricValue(long rawMetricValue) =>
			(long)TimeSpan.FromTicks(rawMetricValue).TotalMilliseconds;

		private CpuTimeMetricsType(string systemName, IMetricsMeasurerCreationHandler handler) : base(systemName, handler)
		{
		}

		protected override CpuTimeMeasurer CreateMeasurerCore()
		{
			return new CpuTimeMeasurer(SystemName);
		}
	}
}
