#nullable disable

using Itc.Commons.Model;

namespace Itc.Commons
{
	public interface IDiagnosticContextMetricsCollection
	{
		void CollectItemData(DiagnosticContextMetricsItem metricsItem);

		void SetTag(string tag, string value);
	}
}
