using System;
using Itc.Commons;
using Itc.Commons.Model;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	public class PrometheusDiagnosticContextFactory : IDiagnosticContextFactory
	{
		private readonly MetricFactory metricFactory;
		private readonly PrometheusMetricNameBuilder metricNameBuilder;

		public PrometheusDiagnosticContextFactory(MetricFactory? metricFactory = null, string? metricPostfix = null)
		{
			this.metricFactory = metricFactory ?? Metrics.WithCustomRegistry(Metrics.DefaultRegistry);
			this.metricNameBuilder = new PrometheusMetricNameBuilder(postfix: metricPostfix);
		}

		public IDiagnosticContext CreateDiagnosticContext(
			string metricPath,
			bool isFeatureBoundaryCodePoint = false,
			MetricsType[]? metricsTypesOverride = null)
		{
			var collection = new PrometheusDiagnosticContextMetricsCollection(metricFactory, metricNameBuilder);
				
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