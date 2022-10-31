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

internal class DiagnosticContextInternalMetricsAdapter
{
	private readonly IMetricFactory _metricFactory;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	public DiagnosticContextInternalMetricsAdapter(IMetricFactory metricFactory, PrometheusMetricNameBuilder metricNameBuilder)
	{
		_metricFactory = metricFactory;
		_metricNameBuilder = metricNameBuilder;
	}

	public void Update(
		DiagnosticContextInternalMetricsItem internalMetricsItem,
		DiagnosticContextMetricsItem collectedMetrics,
		IDictionary<string, string> tags)
	{
		var labelNames = tags.Keys.ToArray();
		var labelValues = tags.Values.ToArray();

		var metricDescriptionBase = $"Diagnostic context {collectedMetrics.MetricPrefix} ";

		var internalProcessingCounter = _metricFactory.CreateCounter(
			_metricNameBuilder.BuildFullMetricName(
				$"{collectedMetrics.MetricPrefix}_{internalMetricsItem.ProcessingTimeMeasurer.MetricTypeSystemName}"),
			$"{metricDescriptionBase} - internal processing time",
			new CounterConfiguration { LabelNames = labelNames });

		internalProcessingCounter
			.WithLabels(labelValues)
			.Inc(internalMetricsItem.ProcessingTimeMeasurer.Elapsed);

		foreach (var measurer in internalMetricsItem.LayersCountMeasurers)
		{
			var layersCounter = _metricFactory.CreateCounter(
				_metricNameBuilder.BuildFullMetricName($"{collectedMetrics.MetricPrefix}_{measurer.MetricTypeSystemName}"),
				$"{metricDescriptionBase} - layers count",
				new CounterConfiguration { LabelNames = labelNames});

			layersCounter
				.WithLabels(labelValues)
				.Inc(measurer.LayersCount);
		}
	}
}