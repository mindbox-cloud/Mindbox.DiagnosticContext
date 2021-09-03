// Copyright 2021 Mindbox Ltd
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage;
using Mindbox.DiagnosticContext.MetricItem;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class DynamicStepsPrometheusAdapter
	{
		private readonly IMetricFactory metricFactory;

		private readonly PrometheusMetricNameBuilder metricNameBuilder;

		private readonly Dictionary<(string, MetricsType), StepPrometheusCounterSet> dynamicStepsPrometheusCounters = 
			new();

		public DynamicStepsPrometheusAdapter(IMetricFactory metricFactory, PrometheusMetricNameBuilder metricNameBuilder)
		{
			this.metricFactory = metricFactory;
			this.metricNameBuilder = metricNameBuilder;
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
						.Inc(metricValue.TotalValue.Count);
					prometheusCounterSet.TotalCounter
						.WithLabels(totalLabelValues)
						.Inc(metricValue.TotalValue.Total);
					
					foreach (var step in metricValue.StepValues.Where(s => s.Value.Total > 0))
					{
						var stepLabelValues = tags.Values
							.Append(step.Key)
							.Append(metricValue.MetricsType.Units)
							.ToArray();
						
						prometheusCounterSet
							.StepCounter
							.WithLabels(stepLabelValues)
							.Inc(step.Value.Total);
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
				var counterConfiguration = new CounterConfiguration {LabelNames = totalLabelNames.ToArray()};

				counterSet = new StepPrometheusCounterSet(
					metricFactory.CreateCounter(
						metricNameBuilder.BuildFullMetricName($"{metricNameBase}_Count"),
						$"{metricDescriptionBase} - total count",
						counterConfiguration),
					metricFactory.CreateCounter(
						metricNameBuilder.BuildFullMetricName($"{metricNameBase}_Total"),
						$"{metricDescriptionBase} - total value",
						counterConfiguration),
					metricFactory.CreateCounter(
						metricNameBuilder.BuildFullMetricName(metricNameBase),
						metricDescriptionBase,
						new CounterConfiguration{LabelNames = stepLabelNames})
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