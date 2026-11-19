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

/// <summary>
/// Publishes the metrics of diagnostic contexts to Prometheus. A factory has one instance, because the adapters
/// keep the metric handles that all the contexts of the factory reuse; contexts publish from many threads at once.
/// <para>
/// When two threads miss the same handle, both build it and one replaces the other. That is safe: prometheus-net
/// returns the same collector for the same name and label names, so both handles wrap one collector.
/// </para>
/// </summary>
internal sealed class PrometheusMetricsPublisher
{
	private readonly TagKeysRegistry _tagKeysRegistry = new();

	private readonly DynamicStepsPrometheusAdapter _dynamicStepsAdapter;
	private readonly CountersPrometheusAdapter _countersAdapter;
	private readonly ReportedValuesPrometheusAdapter _reportedValuesAdapter;
	private readonly DiagnosticContextInternalMetricsAdapter _internalMetricsAdapter;

	public PrometheusMetricsPublisher(
		DiagnosticMetricCreator metricCreator,
		PrometheusMetricNameBuilder metricNameBuilder)
	{
		_dynamicStepsAdapter = new DynamicStepsPrometheusAdapter(metricCreator, metricNameBuilder);
		_countersAdapter = new CountersPrometheusAdapter(metricCreator, metricNameBuilder);
		_reportedValuesAdapter = new ReportedValuesPrometheusAdapter(metricCreator, metricNameBuilder);
		_internalMetricsAdapter = new DiagnosticContextInternalMetricsAdapter(metricCreator, metricNameBuilder);
	}

	public void Publish(
		DiagnosticContextMetricsItem metricsItem,
		DiagnosticContextMetricsStorage storage,
		Dictionary<string, string> tags)
	{
		var tagKeys = _tagKeysRegistry.Get(tags);

		_dynamicStepsAdapter.Update(metricsItem, storage, tags, tagKeys);
		_countersAdapter.Update(metricsItem, storage, tags, tagKeys);
		_reportedValuesAdapter.Update(metricsItem, storage, tags, tagKeys);
	}

	public void PublishInternalMetrics(
		DiagnosticContextInternalMetricsItem internalMetricsItem,
		DiagnosticContextMetricsItem collectedMetrics,
		Dictionary<string, string> tags)
	{
		_internalMetricsAdapter.Update(internalMetricsItem, collectedMetrics, tags, _tagKeysRegistry.Get(tags));
	}
}
