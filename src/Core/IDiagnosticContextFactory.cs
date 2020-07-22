using Itc.Commons;
using Itc.Commons.Model;

namespace Mindbox.DiagnosticContext
{
	public interface IDiagnosticContextFactory
	{
		IDiagnosticContext CreateDiagnosticContext(
			string metricPath,
			bool isFeatureBoundaryCodePoint = false,
			MetricsType[] metricsTypesOverride = null);
	}
}