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

using System;
using System.Collections.Generic;
using Mindbox.DiagnosticContext.MetricItem;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus;

internal class PrometheusDiagnosticContextMetricsCollection : IDiagnosticContextMetricsCollection
{
	private readonly Dictionary<string, DiagnosticContextMetricsStorage> _storagesByMetricPrefix = new();

	private readonly IDictionary<string, string> _tags = new Dictionary<string, string>();

	private readonly DynamicStepsPrometheusAdapter _dynamicStepsAdapter;
	private readonly CountersPrometheusAdapter _countersAdapter;
	private readonly ReportedValuesPrometheusAdapter _reportedValuesAdapter;
	private readonly DiagnosticContextInternalMetricsAdapter _internalMetricsAdapter;

	private readonly object _syncRoot = new();

	public PrometheusDiagnosticContextMetricsCollection(
		IMetricFactory metricFactory,
		PrometheusMetricNameBuilder metricNameBuilder)
	{
		_dynamicStepsAdapter = new DynamicStepsPrometheusAdapter(metricFactory, metricNameBuilder);
		_countersAdapter = new CountersPrometheusAdapter(metricFactory, metricNameBuilder);
		_reportedValuesAdapter = new ReportedValuesPrometheusAdapter(metricFactory, metricNameBuilder);
		_internalMetricsAdapter = new DiagnosticContextInternalMetricsAdapter(metricFactory, metricNameBuilder);
	}

	public void CollectItemData(DiagnosticContextMetricsItem metricsItem)
	{
		try
		{
			lock (_syncRoot)
			{
				var storage = GetOrCreateMetricStorageForMetric(metricsItem);
				storage.CollectItemData(metricsItem);

				_dynamicStepsAdapter.Update(metricsItem, storage, _tags);
				_countersAdapter.Update(metricsItem, storage, _tags);
				_reportedValuesAdapter.Update(metricsItem, storage, _tags);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	public void CollectDiagnosticContextInternalMetrics(
		DiagnosticContextInternalMetricsItem internalMetricsItem,
		DiagnosticContextMetricsItem collectedMetrics)
	{
		_internalMetricsAdapter.Update(internalMetricsItem, collectedMetrics, _tags);
	}

	public void SetTag(string tag, string value)
	{
		_tags[tag] = value;
	}

	private DiagnosticContextMetricsStorage GetOrCreateMetricStorageForMetric(DiagnosticContextMetricsItem metricsItem)
	{
		if (!_storagesByMetricPrefix.TryGetValue(metricsItem.MetricPrefix, out var storage))
		{
			storage = new DiagnosticContextMetricsStorage();
			_storagesByMetricPrefix[metricsItem.MetricPrefix] = storage;
		}

		return storage;
	}
}