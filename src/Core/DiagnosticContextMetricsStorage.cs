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

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage;
using Mindbox.DiagnosticContext.MetricItem;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext;

public class DiagnosticContextMetricsStorage
{
	private readonly Dictionary<string, DiagnosticContextDynamicStepsAggregatedStorage> _dynamicStepsPerMetricPrefix = [];

	private readonly Dictionary<string, DiagnosticContextCountersStorage> _countersPerMetricsPrefix = [];

	private readonly Dictionary<string, DiagnosticContextReportedValuesStorage> _reportedValuesPerMetricsPrefix = [];

	public IReadOnlyDictionary<string, DiagnosticContextCountersStorage> CountersPerMetricsPrefix => _countersPerMetricsPrefix;

	public IReadOnlyDictionary<string, DiagnosticContextReportedValuesStorage> ReportedValuesPerMetricsPrefix
		=> _reportedValuesPerMetricsPrefix;

	public IReadOnlyDictionary<string, DiagnosticContextDynamicStepsAggregatedStorage> DynamicStepsPerMetricPrefix
		=> _dynamicStepsPerMetricPrefix;

	public bool HasData => DynamicStepsPerMetricPrefix.Any()
		|| CountersPerMetricsPrefix.Any()
		|| ReportedValuesPerMetricsPrefix.Any();

	public void CollectItemData(DiagnosticContextMetricsItem item)
	{
		if (item == null)
			throw new ArgumentNullException(nameof(item));

		GetStorageForMetricPrefix(item.MetricsTypes, item.MetricPrefix).CollectItemData(item);

		GetCounterStorageForMetricPrefix(item.MetricPrefix).CollectItemData(item.Counters);

		GetReportedValuesStorageForMetricPrefix(item.MetricPrefix).CollectItemData(item.ReportedValues);
	}

	private DiagnosticContextCountersStorage GetCounterStorageForMetricPrefix(string prefix)
	{
		if (!_countersPerMetricsPrefix.ContainsKey(prefix))
		{
			_countersPerMetricsPrefix.Add(prefix, new DiagnosticContextCountersStorage());
		}
		return _countersPerMetricsPrefix[prefix];
	}

	private DiagnosticContextDynamicStepsAggregatedStorage GetStorageForMetricPrefix(
		MetricsTypeCollection metricsTypes, string prefix)
	{
		if (!_dynamicStepsPerMetricPrefix.ContainsKey(prefix))
		{
			_dynamicStepsPerMetricPrefix.Add(prefix, new DiagnosticContextDynamicStepsAggregatedStorage(metricsTypes));
		}

		return _dynamicStepsPerMetricPrefix[prefix];
	}

	private DiagnosticContextReportedValuesStorage GetReportedValuesStorageForMetricPrefix(string prefix)
	{
		if (!_reportedValuesPerMetricsPrefix.ContainsKey(prefix))
		{
			_reportedValuesPerMetricsPrefix.Add(prefix, new DiagnosticContextReportedValuesStorage());
		}

		return _reportedValuesPerMetricsPrefix[prefix];
	}
}