using Itc.Commons;
using Itc.Commons.Model;

namespace Mindbox.DiagnosticContext
{
	public class NullDiagnosticContextFactory
	{
		IDiagnosticContext CreateDiagnosticContext(
			string metricPath,
			bool isFeatureBoundaryCodePoint = false,
			MetricsType[]? metricsTypesOverride = null)
		{
			return new NullDiagnosticContext();
		}
	}
}