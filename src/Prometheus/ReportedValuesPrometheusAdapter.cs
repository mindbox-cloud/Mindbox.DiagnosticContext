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
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.Prometheus;

internal class ReportedValuesPrometheusAdapter
{
	private readonly DiagnosticMetricCreator _metricCreator;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	private struct ReportedValuesCounters
	{
		public ILabeledMetric Count { get; set; }

		public ILabeledMetric Total { get; set; }
	}

	private readonly Dictionary<string, ReportedValuesCounters> _counters =
		new();

	public ReportedValuesPrometheusAdapter(DiagnosticMetricCreator metricCreator, PrometheusMetricNameBuilder metricNameBuilder)
	{
		_metricCreator = metricCreator;
		_metricNameBuilder = metricNameBuilder;
	}

	public void Update(
		DiagnosticContextMetricsItem metricsItem,
		DiagnosticContextMetricsStorage storage,
		IDictionary<string, string> tags)
	{
		foreach (var reportedValueCounters in storage.ReportedValuesPerMetricsPrefix)
		{
			if (reportedValueCounters.Value.ReportedValues.Any())
			{
				var prometheusCounter = GetOrCreateReportedValuesCounters(
					metricsItem, reportedValueCounters.Key, tags);

				foreach (var diagnosticContextCounter in reportedValueCounters.Value.ReportedValues)
				{
					var labelValues = new List<string> { diagnosticContextCounter.Key };
					labelValues.AddRange(tags.Values);

					var labelValuesArray = labelValues.ToArray();

					prometheusCounter.Total.Inc(labelValuesArray, diagnosticContextCounter.Value.Total);
					prometheusCounter.Count.Inc(labelValuesArray, diagnosticContextCounter.Value.Count);
				}
			}
		}
	}

	private ReportedValuesCounters GetOrCreateReportedValuesCounters(DiagnosticContextMetricsItem metricsItem,
		string counterName, IDictionary<string, string> tags)
	{
		if (_counters.TryGetValue(counterName, out var prometheusCounter)) return prometheusCounter;

		var totalMetricName = _metricNameBuilder.BuildFullMetricName($"{metricsItem.MetricPrefix}_reportedvalues_total");
		var totalMetricDescription = $"Diagnostic context reported values total for {metricsItem.MetricPrefix}";

		var reportedValuesMetricName = _metricNameBuilder
			.BuildFullMetricName($"{metricsItem.MetricPrefix}_reportedvalues_count");
		var reportedValuesMetricDescription = _metricNameBuilder
			.BuildFullMetricName($"Diagnostic context reported values count for {metricsItem.MetricPrefix}");

		var labelNames = new List<string> { "name" };
		labelNames.AddRange(tags.Keys);
		var labelNamesArray = labelNames.ToArray();

		_counters[counterName] = new ReportedValuesCounters
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

		return _counters[counterName];
	}
}
