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
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus;

internal class ReportedValuesPrometheusAdapter
{
	private readonly IMetricFactory _metricFactory;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	private struct ReportedValuesCounters
	{
		public Counter Count { get; set; }

		public Counter Total { get; set; }
	}

	private readonly Dictionary<string, ReportedValuesCounters> _counters = [];

	public ReportedValuesPrometheusAdapter(IMetricFactory metricFactory, PrometheusMetricNameBuilder metricNameBuilder)
	{
		_metricFactory = metricFactory;
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
					string[] labelValuesArray = [diagnosticContextCounter.Key, .. tags.Values];

					prometheusCounter
						.Total
						.WithLabels(labelValuesArray)
						.Inc(diagnosticContextCounter.Value.Total);

					prometheusCounter
						.Count
						.WithLabels(labelValuesArray)
						.Inc(diagnosticContextCounter.Value.Count);
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

		var counterConfiguration = new CounterConfiguration { LabelNames = ["name", .. tags.Keys] };

		_counters[counterName] = new ReportedValuesCounters
		{
			Total = _metricFactory.CreateCounter(
				totalMetricName,
				totalMetricDescription,
				counterConfiguration),
			Count = _metricFactory.CreateCounter(
				reportedValuesMetricName,
				reportedValuesMetricDescription,
				counterConfiguration)
		};

		return _counters[counterName];
	}
}