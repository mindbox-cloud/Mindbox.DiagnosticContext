using System.Collections.Generic;
using System.Linq;
using Itc.Commons.Model;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class ReportedValuesPrometheusAdapter
	{
		private readonly MetricFactory metricFactory;

		private struct ReportedValuesCounters
		{
			public Counter Count { get; set; }

			public Counter Total { get; set; }
		}

		private readonly Dictionary<string, ReportedValuesCounters> counters =
			new Dictionary<string, ReportedValuesCounters>();

		public ReportedValuesPrometheusAdapter(MetricFactory metricFactory)
		{
			this.metricFactory = metricFactory;
		}

		public void Update(DiagnosticContextMetricsItem metricsItem, DiagnosticContextMetricsStorage storage)
		{
			foreach (var reportedValueCounters in storage.ReportedValuesPerMetricsPrefix)
			{
				if (reportedValueCounters.Value.ReportedValues.Any())
				{
					var prometheusCounter = GetOrCreateReportedValuesCounters(metricsItem, reportedValueCounters.Key);

					foreach (var diagnosticContextCounter in reportedValueCounters.Value.ReportedValues)
					{
						prometheusCounter
							.Total
							.WithLabels(diagnosticContextCounter.Key)
							.Inc(diagnosticContextCounter.Value.Total);

						prometheusCounter
							.Count
							.WithLabels(diagnosticContextCounter.Key)
							.Inc(diagnosticContextCounter.Value.Count);
					}
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
				Total = metricFactory.CreateCounter(
					totalMetricName,
					totalMetricDescription,
					"name"),
				Count =  metricFactory.CreateCounter(
					reportedValuesMetricName,
					reportedValuesMetricDescription,
					"name")
			};

			return prometheusCounter;
		}
	}
}