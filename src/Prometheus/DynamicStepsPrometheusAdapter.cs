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

namespace Mindbox.DiagnosticContext.Prometheus;

internal class DynamicStepsPrometheusAdapter
{
	private readonly DiagnosticMetricCreator _metricCreator;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	private readonly Dictionary<(string, MetricsType), StepPrometheusCounterSet> _dynamicStepsPrometheusCounters =
		new();

	public DynamicStepsPrometheusAdapter(DiagnosticMetricCreator metricCreator, PrometheusMetricNameBuilder metricNameBuilder)
	{
		_metricCreator = metricCreator;
		_metricNameBuilder = metricNameBuilder;
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
					.Inc(totalLabelValues, metricValue.TotalValue.Count);
				prometheusCounterSet.TotalCounter
					.Inc(totalLabelValues, metricValue.TotalValue.Total);

				foreach (var step in metricValue.StepValues.Where(s => s.Value.Total > 0))
				{
					var stepLabelValues = tags.Values
						.Append(step.Key)
						.Append(metricValue.MetricsType.Units)
						.ToArray();

					prometheusCounterSet.StepCounter
						.Inc(stepLabelValues, step.Value.Total);
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

		if (!_dynamicStepsPrometheusCounters.TryGetValue(counterSetKey, out var counterSet))
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
				_metricCreator.CreateCounter(
					_metricNameBuilder.BuildFullMetricName($"{metricNameBase}_Count"),
					$"{metricDescriptionBase} - total count",
					totalLabelNames),
				_metricCreator.CreateCounter(
					_metricNameBuilder.BuildFullMetricName($"{metricNameBase}_Total"),
					$"{metricDescriptionBase} - total value",
					totalLabelNames),
				_metricCreator.CreateCounter(
					_metricNameBuilder.BuildFullMetricName(metricNameBase),
					metricDescriptionBase,
					stepLabelNames));

			_dynamicStepsPrometheusCounters[counterSetKey] = counterSet;
		}

		return counterSet;
	}

	private class StepPrometheusCounterSet
	{
		public ILabeledMetric CountCounter { get; }
		public ILabeledMetric TotalCounter { get; }
		public ILabeledMetric StepCounter { get; }

		public StepPrometheusCounterSet(
			ILabeledMetric countCounter,
			ILabeledMetric totalCounter,
			ILabeledMetric stepCounter)
		{
			CountCounter = countCounter;
			TotalCounter = totalCounter;
			StepCounter = stepCounter;
		}
	}
}
