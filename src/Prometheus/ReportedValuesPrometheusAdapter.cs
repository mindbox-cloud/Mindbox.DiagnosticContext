using System.Collections.Generic;
using System.Linq;
using Itc.Commons.Model;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class ReportedValuesPrometheusAdapter
	{
		private readonly MetricFactory metricFactory;

		private readonly PrometheusMetricNameBuilder metricNameBuilder;

		private struct ReportedValuesCounters
		{
			public Counter Count { get; set; }

			public Counter Total { get; set; }
		}

		private readonly Dictionary<string, ReportedValuesCounters> counters =
			new Dictionary<string, ReportedValuesCounters>();

		public ReportedValuesPrometheusAdapter(MetricFactory metricFactory, PrometheusMetricNameBuilder metricNameBuilder)
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
			var labelNamesArray = labelNames.ToArray();
			
			counters[counterName] = new ReportedValuesCounters
			{
				Total = metricFactory.CreateCounter(
					totalMetricName,
					totalMetricDescription,
					labelNamesArray),
				Count =  metricFactory.CreateCounter(
					reportedValuesMetricName,
					reportedValuesMetricDescription,
					labelNamesArray)
			};

			return counters[counterName];
		}
	}
}