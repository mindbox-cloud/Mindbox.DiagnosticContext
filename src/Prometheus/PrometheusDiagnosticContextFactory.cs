using System;
using Itc.Commons;
using Itc.Commons.Model;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	public class PrometheusDiagnosticContextFactory : IDiagnosticContextFactory
	{
		private readonly MetricFactory metricFactory;

		public PrometheusDiagnosticContextFactory(MetricFactory? metricFactory = null)
		{
			this.metricFactory = metricFactory ?? Metrics.WithCustomRegistry(Metrics.DefaultRegistry);
		}

		public PrometheusDiagnosticContextFactory(CollectorRegistry metricRegistry)
		{
			this.metricFactory = Metrics.WithCustomRegistry(metricRegistry);
		}

		public IDiagnosticContext CreateDiagnosticContext(
			string metricPath,
			bool isFeatureBoundaryCodePoint = false,
			MetricsType[]? metricsTypesOverride = null)
		{
			var collection = new PrometheusDiagnosticContextMetricsCollection(metricFactory);
				
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