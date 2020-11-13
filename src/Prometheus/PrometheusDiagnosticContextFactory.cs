using Itc.Commons;
using Itc.Commons.Model;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	public class PrometheusDiagnosticContextFactory : IDiagnosticContextFactory
	{
		private readonly PrometheusDiagnosticContextMetricsCollection collection;

		public PrometheusDiagnosticContextFactory(MetricFactory metricFactory = null)
		{
			collection = new PrometheusDiagnosticContextMetricsCollection(
				metricFactory ?? Metrics.WithCustomRegistry(Metrics.DefaultRegistry));
		}

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