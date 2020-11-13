using System.Collections.Generic;

using Itc.Commons.Model;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class CountersPrometheusAdapter
	{
		private readonly MetricFactory metricFactory;

		private readonly Dictionary<string, Counter> prometheusCounters =
			new Dictionary<string, Counter>();

		public CountersPrometheusAdapter(MetricFactory metricFactory)
		{
			this.metricFactory = metricFactory;
		}

		public void Update(DiagnosticContextMetricsItem metricsItem, DiagnosticContextMetricsStorage storage)
		{
			foreach (var prefixCounters in storage.CountersPerMetricsPrefix)
			{
				var prometheusCounter = GetOrCreatePrometheusCounter(metricsItem, prefixCounters.Key);

				foreach (var diagnosticContextCounter in prefixCounters.Value.Counters)
				{
					prometheusCounter
						.WithLabels(diagnosticContextCounter.Key)
						.IncTo(diagnosticContextCounter.Value);
				}
			}
		}
		
		private Counter GetOrCreatePrometheusCounter(
			DiagnosticContextMetricsItem metricsItem,
			string counterName)
		{	
			if (!prometheusCounters.TryGetValue(counterName, out var prometheusCounter))
			{
				string metricName = MetricNameHelper.BuildFullMetricName($"{metricsItem.MetricPrefix}_counters");
				string metricDescription = $"Diagnostic context counters for {metricsItem.MetricPrefix}";

				prometheusCounter = metricFactory.CreateCounter(
					metricName,
					metricDescription,
					"name");

				prometheusCounters[counterName] = prometheusCounter;
			}

			return prometheusCounter;
		}
	}
}