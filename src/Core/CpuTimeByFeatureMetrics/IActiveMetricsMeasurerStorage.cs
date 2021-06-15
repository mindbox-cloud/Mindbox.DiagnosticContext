#nullable disable

namespace Mindbox.DiagnosticContext.CpuTimeByFeatureMetrics
{
	internal interface IActiveMetricsMeasurerStorage
	{
		MetricsMeasurer GetActiveMeasurer();
		void SetActiveMeasurer(MetricsMeasurer measurer);
	}
}
