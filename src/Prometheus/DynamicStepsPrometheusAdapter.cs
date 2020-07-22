using System.Collections.Generic;

using Itc.Commons;
using Itc.Commons.Model;

using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class DynamicStepsPrometheusAdapter
	{	
		private readonly Dictionary<(string, MetricsType), StepPrometheusCounterSet> dynamicStepsPrometheusCounters = 
			new Dictionary<(string, MetricsType), StepPrometheusCounterSet>();
		
		public void Update(DiagnosticContextMetricsItem metricsItem, DiagnosticContextMetricsStorage storage)
		{
			foreach (var prefixSteps in storage.DynamicStepsPerMetricPrefix)
			{
				foreach (var metricValue in prefixSteps.Value.MetricsAggregatedValues.GetMetricsAggregatedValues())
				{	
					var prometheusCounterSet = GetOrCreateMetricCounterSet(metricsItem, metricValue);

					prometheusCounterSet.CountCounter.IncTo(metricValue.TotalValue.Count);
					prometheusCounterSet.TotalCounter.IncTo(metricValue.TotalValue.Total);
					
					foreach (var step in metricValue.StepValues)
					{
						prometheusCounterSet
							.StepCounter
							.WithLabels(step.Key, metricValue.MetricsType.Units)
							.IncTo(step.Value.Total);
					}
				}
			}
			
		}
		
		private StepPrometheusCounterSet GetOrCreateMetricCounterSet(
			DiagnosticContextMetricsItem metricsItem,
			MetricsAggregatedValue metricValue)
		{
			var counterSetKey = (metricsItem.MetricPrefix, metricValue.MetricsType);
			
			if (!dynamicStepsPrometheusCounters.TryGetValue(counterSetKey, out var counterSet))
			{
				string metricNameBase = $"{metricsItem.MetricPrefix}_{metricValue.MetricsType.SystemName}";
				string metricDescriptionBase =
					$"Diagnostic context for {metricsItem.MetricPrefix} ({metricValue.MetricsType.SystemName})";

				counterSet = new StepPrometheusCounterSet(
					Metrics.CreateCounter(
						MetricNameHelper.BuildFullMetricName($"{metricNameBase}_Count"),
						$"{metricDescriptionBase} - total count"),
					Metrics.CreateCounter(
						MetricNameHelper.BuildFullMetricName($"{metricNameBase}_Total"),
						$"{metricDescriptionBase} - total value"),
					Metrics.CreateCounter(
						MetricNameHelper.BuildFullMetricName(metricNameBase),
						metricDescriptionBase,
						"step",
						"unit"));

				dynamicStepsPrometheusCounters[counterSetKey] = counterSet;
			}

			return counterSet;
		}

		private class StepPrometheusCounterSet
		{
			public Counter CountCounter { get; }
			public Counter TotalCounter { get; }
			public Counter StepCounter { get; }

			public StepPrometheusCounterSet(Counter countCounter, Counter totalCounter, Counter stepCounter)
			{
				CountCounter = countCounter;
				TotalCounter = totalCounter;
				StepCounter = stepCounter;
			}
		}
	}
}