#nullable disable

using System.Threading;

namespace Mindbox.DiagnosticContext.CpuTimeByFeatureMetrics
{
	internal sealed class DefaultActiveMetricsMeasurerStorage : IActiveMetricsMeasurerStorage
	{
		private static readonly ThreadLocal<MetricsMeasurer> measurer = new ThreadLocal<MetricsMeasurer>();

		public MetricsMeasurer GetActiveMeasurer()
		{
			return measurer.Value;
		}

		public void SetActiveMeasurer(MetricsMeasurer newMeasurer)
		{
			measurer.Value = newMeasurer;
		}
	}
}
