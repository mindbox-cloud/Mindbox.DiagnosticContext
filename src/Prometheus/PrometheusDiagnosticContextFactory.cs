using Itc.Commons;
using Itc.Commons.Model;

namespace Mindbox.DiagnosticContext.Prometheus
{
	public class PrometheusDiagnosticContextFactory : IDiagnosticContextFactory
	{
		private static readonly PrometheusDiagnosticContextMetricsCollection collection =
			new PrometheusDiagnosticContextMetricsCollection();

		public IDiagnosticContext CreateDiagnosticContext(
			string metricPath,
			bool isFeatureBoundaryCodePoint = false,
			MetricsType[] metricsTypesOverride = null)
		{
			return DiagnosticContextFactory.BuildCustom(
				() =>
				{
					var metricTypes = metricsTypesOverride
							?.Transform(types => new MetricsTypeCollection(types))
						?? DefaultMetricTypesConfiguration.Instance.GetMetricsTypesWithCpuTimeTracking(metricPath);

					return new Itc.Commons.Model.DiagnosticContext(
						metricPath,
						collection,
						metricTypes,
						isFeatureBoundaryCodePoint);
				});
		}
	}
}