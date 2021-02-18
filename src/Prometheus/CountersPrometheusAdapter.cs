using System.Collections.Generic;
using System.Linq;
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

		public void Update(
			DiagnosticContextMetricsItem metricsItem, 
			DiagnosticContextMetricsStorage storage,
			IDictionary<string, string> tags)
		{
			foreach (var prefixCounters in storage.CountersPerMetricsPrefix)
			{
				var prometheusCounter = GetOrCreatePrometheusCounter(metricsItem, prefixCounters.Key, tags);

				foreach (var diagnosticContextCounter in prefixCounters.Value.Counters)
				{
					var labelValues = new List<string> {diagnosticContextCounter.Key};
					labelValues.AddRange(tags.Values);
					
					prometheusCounter
						.WithLabels(labelValues.ToArray())
						.Inc(diagnosticContextCounter.Value);
				}
			}
		}
		
		private Counter GetOrCreatePrometheusCounter(
			DiagnosticContextMetricsItem metricsItem,
			string counterName, 
			IDictionary<string, string> tags)
		{	
			if (!prometheusCounters.TryGetValue(counterName, out var prometheusCounter))
			{
				string metricName = MetricNameHelper.BuildFullMetricName($"{metricsItem.MetricPrefix}_counters");
				string metricDescription = $"Diagnostic context counters for {metricsItem.MetricPrefix}";

				var labelNames = new List<string> {"name"};
				labelNames.AddRange(tags.Keys);
				
				prometheusCounter = metricFactory.CreateCounter(
					metricName,
					metricDescription,
					labelNames.ToArray());

				prometheusCounters[counterName] = prometheusCounter;
			}

			return prometheusCounter;
		}
	}
}