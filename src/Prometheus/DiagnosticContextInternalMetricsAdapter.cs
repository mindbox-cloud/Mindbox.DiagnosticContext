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

internal class DiagnosticContextInternalMetricsAdapter
{
	private readonly DiagnosticMetricCreator _metricCreator;

	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

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
		IDictionary<string, string> tags)
	{
		var labelNames = tags.Keys.ToArray();
		var labelValues = tags.Values.ToArray();

		var metricDescriptionBase = $"Diagnostic context {collectedMetrics.MetricPrefix} ";

		var countCounter = _metricCreator.CreateCounter(
			_metricNameBuilder.BuildFullMetricName(
			$"{collectedMetrics.MetricPrefix}_internalmetrics_count"),
			$"{metricDescriptionBase} - internal metrics count",
			labelNames);

		countCounter.Inc(labelValues, 1);

		var internalProcessingCounter = _metricCreator.CreateCounter(
			_metricNameBuilder.BuildFullMetricName(
			$"{collectedMetrics.MetricPrefix}_{internalMetricsItem.ProcessingTimeMeasurer.MetricTypeSystemName}"),
			$"{metricDescriptionBase} - internal processing time",
			labelNames);

		internalProcessingCounter.Inc(labelValues, internalMetricsItem.ProcessingTimeMeasurer.Elapsed);

		foreach (var measurer in internalMetricsItem.LayersCountMeasurers)
		{
			var layersCounter = _metricCreator.CreateCounter(
				_metricNameBuilder.BuildFullMetricName(
					$"{collectedMetrics.MetricPrefix}_{measurer.MetricTypeSystemName}"),
				$"{metricDescriptionBase} - layers count",
				labelNames);

			layersCounter.Inc(labelValues, measurer.LayersCount);
		}
	}
}
