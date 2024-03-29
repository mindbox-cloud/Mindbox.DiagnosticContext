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

namespace Mindbox.DiagnosticContext.MetricsTypes;

public class DiagnosticContextLayersCountMeasurer
{
	private readonly string _collectedMetricTypeSystemName;
	private const string LayersCountMetricType = "LayersCount";
	private bool _isFinished;
	private long _layersCount;

	public DiagnosticContextLayersCountMeasurer(string collectedMetricTypeSystemName)
	{
		_collectedMetricTypeSystemName = collectedMetricTypeSystemName;
	}

	public long LayersCount
	{
		get => _layersCount;
		private set
		{
			_layersCount = value;
			_isFinished = true;
		}
	}

	public string MetricTypeSystemName => $"{_collectedMetricTypeSystemName}_{LayersCountMetricType}";

	public void Measure(DiagnosticContextMetricsItem metricsItem)
	{
		if (_isFinished)
			throw new InvalidOperationException("Cannot use one measurer twice");

		var normalizedMetricValue = metricsItem
			.GetNormalizedMetricsValues()
			.GetValueByMetricsTypeSystemName(_collectedMetricTypeSystemName);

		LayersCount = normalizedMetricValue.NormalizedValues.Count;
	}
}

public static class DiagnosticContextLayersCountMeasurerExtensions
{
	public static void Measure(
		this IEnumerable<DiagnosticContextLayersCountMeasurer> measurers,
		DiagnosticContextMetricsItem metricsItem)
	{
		foreach (var layersCountMeasurer in measurers)
		{
			layersCountMeasurer.Measure(metricsItem);
		}
	}
}