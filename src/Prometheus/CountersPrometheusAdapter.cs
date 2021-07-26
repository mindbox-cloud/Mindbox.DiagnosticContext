using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.MetricItem;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class CountersPrometheusAdapter
	{
		private readonly IMetricFactory metricFactory;

		private readonly PrometheusMetricNameBuilder metricNameBuilder;

		private readonly Dictionary<string, Counter> prometheusCounters = new();

		public CountersPrometheusAdapter(IMetricFactory metricFactory, PrometheusMetricNameBuilder metricNameBuilder)
		{
			this.metricFactory = metricFactory;
			this.metricNameBuilder = metricNameBuilder;
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
				string metricName = metricNameBuilder.BuildFullMetricName($"{metricsItem.MetricPrefix}_counters");
				string metricDescription = $"Diagnostic context counters for {metricsItem.MetricPrefix}";

				var labelNames = new List<string> {"name"};
				labelNames.AddRange(tags.Keys);
				
				prometheusCounter = metricFactory.CreateCounter(
					metricName,
					metricDescription,
					new CounterConfiguration(){LabelNames = labelNames.ToArray()});

				prometheusCounters[counterName] = prometheusCounter;
			}

			return prometheusCounter;
		}
	}
}