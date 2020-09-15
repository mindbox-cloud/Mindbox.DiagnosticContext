using System.Collections.Generic;

using Itc.Commons.Model;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class ReportedValuesPrometheusAdapter
	{
		private struct ReportedValuesCounters
		{
			public Counter Count { get; set; }

			public Counter Total { get; set; }
		}

		private readonly Dictionary<string, ReportedValuesCounters> counters =
			new Dictionary<string, ReportedValuesCounters>();

		public void Update(DiagnosticContextMetricsItem metricsItem, DiagnosticContextMetricsStorage storage)
		{
			foreach (var reportedValueCounters in storage.ReportedValuesPerMetricsPrefix)
			{
				var prometheusCounter = GetOrCreateReportedValuesCounters(metricsItem, reportedValueCounters.Key);

				foreach (var diagnosticContextCounter in reportedValueCounters.Value.ReportedValues)
				{
					prometheusCounter
						.Total
						.WithLabels(diagnosticContextCounter.Key)
						.IncTo(diagnosticContextCounter.Value.Total);

					prometheusCounter
						.Count
						.WithLabels(diagnosticContextCounter.Key)
						.IncTo(diagnosticContextCounter.Value.Count);
				}
			}
		}

		private ReportedValuesCounters GetOrCreateReportedValuesCounters(
			DiagnosticContextMetricsItem metricsItem,
			string counterName)
		{
			if (counters.TryGetValue(counterName, out var prometheusCounter)) return prometheusCounter;

			var totalMetricName = MetricNameHelper.BuildFullMetricName($"{metricsItem.MetricPrefix}_reportedvalues_total");
			var totalMetricDescription = $"Diagnostic context reported values total for {metricsItem.MetricPrefix}";

			var reportedValuesMetricName = MetricNameHelper
				.BuildFullMetricName($"{metricsItem.MetricPrefix}_reportedvalues_count");
			var reportedValuesMetricDescription = MetricNameHelper
				.BuildFullMetricName($"Diagnostic context reported values count for {metricsItem.MetricPrefix}");

			counters[counterName] = new ReportedValuesCounters
			{
				Total = Metrics.CreateCounter(
					totalMetricName,
					totalMetricDescription,
					"name"),
				Count =  Metrics.CreateCounter(
					reportedValuesMetricName,
					reportedValuesMetricDescription,
					"name")
			};

			return prometheusCounter;
		}
	}
}