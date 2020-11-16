using System.Collections.Generic;
using System.Linq;
using Itc.Commons;
using Itc.Commons.Model;

using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class DynamicStepsPrometheusAdapter
	{
		private readonly MetricFactory metricFactory;

		private readonly Dictionary<(string, MetricsType), StepPrometheusCounterSet> dynamicStepsPrometheusCounters = 
			new Dictionary<(string, MetricsType), StepPrometheusCounterSet>();

		public DynamicStepsPrometheusAdapter(MetricFactory metricFactory)
		{
			this.metricFactory = metricFactory;
		}

		public void Update(
			DiagnosticContextMetricsItem metricsItem, 
			DiagnosticContextMetricsStorage storage, 
			IDictionary<string, string> tags)
		{
			foreach (var prefixSteps in storage.DynamicStepsPerMetricPrefix)
			{
				foreach (var metricValue in prefixSteps.Value.MetricsAggregatedValues.GetMetricsAggregatedValues())
				{	
					var prometheusCounterSet = GetOrCreateMetricCounterSet(metricsItem, metricValue, tags);

					var totalLabelValues = tags.Values.ToArray();
					
					prometheusCounterSet.CountCounter
						.WithLabels(totalLabelValues)
						.IncTo(metricValue.TotalValue.Count);
					prometheusCounterSet.TotalCounter
						.WithLabels(totalLabelValues)
						.IncTo(metricValue.TotalValue.Total);
					
					foreach (var step in metricValue.StepValues.Where(s => s.Value.Total > 0))
					{
						var stepLabelValues = tags.Values
							.Append(step.Key)
							.Append(metricValue.MetricsType.Units)
							.ToArray();
						
						prometheusCounterSet
							.StepCounter
							.WithLabels(stepLabelValues)
							.IncTo(step.Value.Total);
					}
				}
			}
			
		}
		
		private StepPrometheusCounterSet GetOrCreateMetricCounterSet(
			DiagnosticContextMetricsItem metricsItem,
			MetricsAggregatedValue metricValue,
			IDictionary<string, string> tags)
		{
			var counterSetKey = (metricsItem.MetricPrefix, metricValue.MetricsType);
			
			if (!dynamicStepsPrometheusCounters.TryGetValue(counterSetKey, out var counterSet))
			{
				string metricNameBase = $"{metricsItem.MetricPrefix}_{metricValue.MetricsType.SystemName}";
				string metricDescriptionBase =
					$"Diagnostic context for {metricsItem.MetricPrefix} ({metricValue.MetricsType.SystemName})";

				var totalLabelNames = tags.Keys.ToArray();
				var stepLabelNames = tags.Keys
					.Append("step")
					.Append("unit")
					.ToArray();
				
				counterSet = new StepPrometheusCounterSet(
					metricFactory.CreateCounter(
						MetricNameHelper.BuildFullMetricName($"{metricNameBase}_Count"),
						$"{metricDescriptionBase} - total count",
						totalLabelNames),
					metricFactory.CreateCounter(
						MetricNameHelper.BuildFullMetricName($"{metricNameBase}_Total"),
						$"{metricDescriptionBase} - total value",
						totalLabelNames),
					metricFactory.CreateCounter(
						MetricNameHelper.BuildFullMetricName(metricNameBase),
						metricDescriptionBase,
						stepLabelNames)
					);

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