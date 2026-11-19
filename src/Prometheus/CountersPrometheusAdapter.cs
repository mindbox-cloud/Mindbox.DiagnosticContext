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
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.Prometheus;

internal class CountersPrometheusAdapter
{
	private readonly DiagnosticMetricCreator _metricCreator;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	private readonly ConcurrentDictionary<(string MetricPrefix, TagKeys TagKeys), ICounterAdapter> _prometheusCounters =
		new();

	public CountersPrometheusAdapter(DiagnosticMetricCreator metricCreator, PrometheusMetricNameBuilder metricNameBuilder)
	{
		_metricCreator = metricCreator;
		_metricNameBuilder = metricNameBuilder;
	}

	public void Update(
		DiagnosticContextMetricsItem metricsItem,
		DiagnosticContextMetricsStorage storage,
		Dictionary<string, string> tags,
		TagKeys tagKeys)
	{
		foreach (var prefixCounters in storage.CountersPerMetricsPrefix)
		{
			var prometheusCounter = GetOrCreatePrometheusCounter(metricsItem, tagKeys);

			foreach (var diagnosticContextCounter in prefixCounters.Value.Counters)
			{
				var labelValues = LabelValues.Prepend(diagnosticContextCounter.Key, tags);

				prometheusCounter.Inc(labelValues, diagnosticContextCounter.Value);
			}
		}
	}

	private ICounterAdapter GetOrCreatePrometheusCounter(DiagnosticContextMetricsItem metricsItem, TagKeys tagKeys)
	{
		var counterKey = (metricsItem.MetricPrefix, tagKeys);

		if (!_prometheusCounters.TryGetValue(counterKey, out var prometheusCounter))
		{
			string metricName = _metricNameBuilder.BuildFullMetricName($"{metricsItem.MetricPrefix}_counters");
			string metricDescription = $"Diagnostic context counters for {metricsItem.MetricPrefix}";

			var labelNames = new List<string> { "name" };
			labelNames.AddRange(tagKeys.Names);

			prometheusCounter = _metricCreator.CreateCounter(
				metricName,
				metricDescription,
				labelNames.ToArray());

			_prometheusCounters[counterKey] = prometheusCounter;
		}

		return prometheusCounter;
	}
}
