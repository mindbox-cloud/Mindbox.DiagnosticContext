#nullable disable

using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext
{
	public interface IDiagnosticContextMetricsCollection
	{
		void CollectItemData(DiagnosticContextMetricsItem metricsItem);

		void SetTag(string tag, string value);
	}
}
