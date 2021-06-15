using System;
using Mindbox.DiagnosticContext.MetricsTypes;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	public class PrometheusDiagnosticContextFactory : IDiagnosticContextFactory
	{
		private readonly DefaultMetricTypesConfiguration defaultMetricTypesConfiguration;
		private readonly IDiagnosticContextLogger diagnosticContextLogger;
		private readonly MetricFactory metricFactory;
		private readonly PrometheusMetricNameBuilder metricNameBuilder;

		public PrometheusDiagnosticContextFactory(DefaultMetricTypesConfiguration defaultMetricTypesConfiguration, IDiagnosticContextLogger diagnosticContextLogger, MetricFactory? metricFactory = null, string? metricPostfix = null)
		{
			this.defaultMetricTypesConfiguration = defaultMetricTypesConfiguration;
			this.diagnosticContextLogger = diagnosticContextLogger;
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
					var metricTypes = metricsTypesOverride != null ?
							new MetricsTypeCollection(metricsTypesOverride) 
							: defaultMetricTypesConfiguration.GetMetricsTypesWithCpuTimeTracking(metricPath);

					return new DiagnosticContext(
						diagnosticContextLogger,
						metricPath,
						collection,
						metricTypes,
						isFeatureBoundaryCodePoint);
				}, diagnosticContextLogger);
		}
	}
}