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
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.Prometheus;

internal class CountersPrometheusAdapter
{
	private readonly DiagnosticMetricCreator _metricCreator;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	private readonly Dictionary<string, ICounterAdapter> _prometheusCounters = new();

	public CountersPrometheusAdapter(DiagnosticMetricCreator metricCreator, PrometheusMetricNameBuilder metricNameBuilder)
	{
		_metricCreator = metricCreator;
		_metricNameBuilder = metricNameBuilder;
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
				var labelValues = new List<string> { diagnosticContextCounter.Key };
				labelValues.AddRange(tags.Values);

				prometheusCounter.Inc(labelValues.ToArray(), diagnosticContextCounter.Value);
			}
		}
	}

	private ICounterAdapter GetOrCreatePrometheusCounter(
		DiagnosticContextMetricsItem metricsItem,
		string counterName,
		IDictionary<string, string> tags)
	{
		if (!_prometheusCounters.TryGetValue(counterName, out var prometheusCounter))
		{
			string metricName = _metricNameBuilder.BuildFullMetricName($"{metricsItem.MetricPrefix}_counters");
			string metricDescription = $"Diagnostic context counters for {metricsItem.MetricPrefix}";

			var labelNames = new List<string> { "name" };
			labelNames.AddRange(tags.Keys);

			prometheusCounter = _metricCreator.CreateCounter(
				metricName,
				metricDescription,
				labelNames.ToArray());

			_prometheusCounters[counterName] = prometheusCounter;
		}

		return prometheusCounter;
	}
}
