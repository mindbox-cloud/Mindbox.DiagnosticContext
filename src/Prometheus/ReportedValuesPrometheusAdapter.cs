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

internal class ReportedValuesPrometheusAdapter
{
	private readonly DiagnosticMetricCreator _metricCreator;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	private struct ReportedValuesCounters
	{
		public ICounterAdapter Count { get; set; }

		public ICounterAdapter Total { get; set; }
	}

	private readonly ConcurrentDictionary<(string MetricPrefix, TagKeys TagKeys), ReportedValuesCounters> _counters =
		new();

	public ReportedValuesPrometheusAdapter(DiagnosticMetricCreator metricCreator, PrometheusMetricNameBuilder metricNameBuilder)
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
		foreach (var reportedValueCounters in storage.ReportedValuesPerMetricsPrefix)
		{
			if (reportedValueCounters.Value.ReportedValues.Any())
			{
				var prometheusCounter = GetOrCreateReportedValuesCounters(metricsItem, tagKeys);

				foreach (var diagnosticContextCounter in reportedValueCounters.Value.ReportedValues)
				{
					var labelValues = LabelValues.Prepend(diagnosticContextCounter.Key, tags);

					prometheusCounter.Total.Inc(labelValues, diagnosticContextCounter.Value.Total);
					prometheusCounter.Count.Inc(labelValues, diagnosticContextCounter.Value.Count);
				}
			}
		}
	}

	private ReportedValuesCounters GetOrCreateReportedValuesCounters(DiagnosticContextMetricsItem metricsItem, TagKeys tagKeys)
	{
		var countersKey = (metricsItem.MetricPrefix, tagKeys);

		if (_counters.TryGetValue(countersKey, out var prometheusCounter)) return prometheusCounter;

		var totalMetricName = _metricNameBuilder.BuildFullMetricName($"{metricsItem.MetricPrefix}_reportedvalues_total");
		var totalMetricDescription = $"Diagnostic context reported values total for {metricsItem.MetricPrefix}";

		var reportedValuesMetricName = _metricNameBuilder
			.BuildFullMetricName($"{metricsItem.MetricPrefix}_reportedvalues_count");
		var reportedValuesMetricDescription = _metricNameBuilder
			.BuildFullMetricName($"Diagnostic context reported values count for {metricsItem.MetricPrefix}");

		var labelNames = new List<string> { "name" };
		labelNames.AddRange(tagKeys.Names);
		var labelNamesArray = labelNames.ToArray();

		prometheusCounter = new ReportedValuesCounters
		{
			Total = _metricCreator.CreateCounter(
				totalMetricName,
				totalMetricDescription,
				labelNamesArray),
			Count = _metricCreator.CreateCounter(
				reportedValuesMetricName,
				reportedValuesMetricDescription,
				labelNamesArray)
		};

		_counters[countersKey] = prometheusCounter;

		return prometheusCounter;
	}
}
