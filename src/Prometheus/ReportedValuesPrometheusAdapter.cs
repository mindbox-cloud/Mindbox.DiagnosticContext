using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.MetricItem;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class ReportedValuesPrometheusAdapter
	{
		private readonly IMetricFactory metricFactory;

		private readonly PrometheusMetricNameBuilder metricNameBuilder;

		private struct ReportedValuesCounters
		{
			public Counter Count { get; set; }

			public Counter Total { get; set; }
		}

		private readonly Dictionary<string, ReportedValuesCounters> counters =
			new();

		public ReportedValuesPrometheusAdapter(IMetricFactory metricFactory, PrometheusMetricNameBuilder metricNameBuilder)
		{
			this.metricFactory = metricFactory;
			this.metricNameBuilder = metricNameBuilder;
		}

		public void Update(
			DiagnosticContextMetricsItem metricsItem, 
			DiagnosticContextMetricsStorage storage,
			IDictionary<string, string> tags)
		{
			foreach (var reportedValueCounters in storage.ReportedValuesPerMetricsPrefix)
			{
				if (reportedValueCounters.Value.ReportedValues.Any())
				{
					var prometheusCounter = GetOrCreateReportedValuesCounters(
						metricsItem, reportedValueCounters.Key, tags);

					foreach (var diagnosticContextCounter in reportedValueCounters.Value.ReportedValues)
					{
						var labelValues = new List<string> {diagnosticContextCounter.Key};
						labelValues.AddRange(tags.Values);

						var labelValuesArray = labelValues.ToArray();
						
						prometheusCounter
							.Total
							.WithLabels(labelValuesArray)
							.Inc(diagnosticContextCounter.Value.Total);

						prometheusCounter
							.Count
							.WithLabels(labelValuesArray)
							.Inc(diagnosticContextCounter.Value.Count);
					}
				}
			}
		}

		private ReportedValuesCounters GetOrCreateReportedValuesCounters(DiagnosticContextMetricsItem metricsItem,
			string counterName, IDictionary<string, string> tags)
		{
			if (counters.TryGetValue(counterName, out var prometheusCounter)) return prometheusCounter;

			var totalMetricName = metricNameBuilder.BuildFullMetricName($"{metricsItem.MetricPrefix}_reportedvalues_total");
			var totalMetricDescription = $"Diagnostic context reported values total for {metricsItem.MetricPrefix}";

			var reportedValuesMetricName = metricNameBuilder
				.BuildFullMetricName($"{metricsItem.MetricPrefix}_reportedvalues_count");
			var reportedValuesMetricDescription = metricNameBuilder
				.BuildFullMetricName($"Diagnostic context reported values count for {metricsItem.MetricPrefix}");

			var labelNames = new List<string> {"name"};
			labelNames.AddRange(tags.Keys);
			var counterConfiguration = new CounterConfiguration{LabelNames = labelNames.ToArray()};
			
			counters[counterName] = new ReportedValuesCounters
			{
				Total = metricFactory.CreateCounter(
					totalMetricName,
					totalMetricDescription,
					counterConfiguration),
				Count =  metricFactory.CreateCounter(
					reportedValuesMetricName,
					reportedValuesMetricDescription,
					counterConfiguration)
			};

			return counters[counterName];
		}
	}
}