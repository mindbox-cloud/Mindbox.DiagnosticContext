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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.Prometheus;

internal class DiagnosticContextInternalMetricsAdapter
{
	private readonly DiagnosticMetricCreator _metricCreator;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	private readonly ConcurrentDictionary<
		(string MetricPrefix, string MetricNameSuffix, TagKeys TagKeys),
		ICounterAdapter> _counters = new();

	public DiagnosticContextInternalMetricsAdapter(
		DiagnosticMetricCreator metricCreator,
		PrometheusMetricNameBuilder metricNameBuilder)
	{
		_metricCreator = metricCreator;
		_metricNameBuilder = metricNameBuilder;
	}

	public void Update(
		DiagnosticContextInternalMetricsItem internalMetricsItem,
		DiagnosticContextMetricsItem collectedMetrics,
		Dictionary<string, string> tags,
		TagKeys tagKeys)
	{
		var metricPrefix = collectedMetrics.MetricPrefix;
		var labelValues = LabelValues.From(tags);

		var countCounter = GetOrCreateCounter(metricPrefix, "internalmetrics_count", "internal metrics count", tagKeys);

		countCounter.Inc(labelValues, 1);

		var internalProcessingCounter = GetOrCreateCounter(
			metricPrefix,
			internalMetricsItem.ProcessingTimeMeasurer.MetricTypeSystemName,
			"internal processing time",
			tagKeys);

		internalProcessingCounter.Inc(labelValues, internalMetricsItem.ProcessingTimeMeasurer.Elapsed);

		foreach (var measurer in internalMetricsItem.LayersCountMeasurers)
		{
			var layersCounter = GetOrCreateCounter(metricPrefix, measurer.MetricTypeSystemName, "layers count", tagKeys);

			layersCounter.Inc(labelValues, measurer.LayersCount);
		}
	}

	private ICounterAdapter GetOrCreateCounter(
		string metricPrefix,
		string metricNameSuffix,
		string description,
		TagKeys tagKeys)
	{
		var counterKey = (metricPrefix, metricNameSuffix, tagKeys);

		if (!_counters.TryGetValue(counterKey, out var counter))
		{
			counter = _metricCreator.CreateCounter(
				_metricNameBuilder.BuildFullMetricName($"{metricPrefix}_{metricNameSuffix}"),
				$"Diagnostic context {metricPrefix}  - {description}",
				tagKeys.Names.ToArray());

			_counters[counterKey] = counter;
		}

		return counter;
	}
}
