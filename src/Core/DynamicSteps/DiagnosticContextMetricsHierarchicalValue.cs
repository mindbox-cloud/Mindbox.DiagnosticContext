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
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.DynamicSteps;

internal class DiagnosticContextMetricsHierarchicalValue
{
	public static DiagnosticContextMetricsHierarchicalValue FromMetricsType(MetricsType metricsType)
	{
		return new DiagnosticContextMetricsHierarchicalValue(metricsType);
	}

	private const string OtherStepName = "Other";

	private IDictionary<string, long> StepValues { get; } = new Dictionary<string, long>();
	private long? TotalValue { get; set; }

	public string MetricsTypeSystemName => _metricsType.SystemName;

	private readonly MetricsType _metricsType;

	private static KeyValuePair<string, long> _lastIncrementedNamedValue;

	private DiagnosticContextMetricsHierarchicalValue(MetricsType metricsType)
	{
		_metricsType = metricsType;
	}

	public void SetTotal(long total)
	{
		if (TotalValue.HasValue)
			throw new InvalidOperationException("TotalValue.HasValue");

		TotalValue = _metricsType.ConvertMetricValue(total);
	}

	public void IncrementMetricsValue(string path, long increment)
	{
		IncrementNamedValue(StepValues, path, _metricsType.ConvertMetricValue(increment));
	}

	private static void IncrementNamedValue(IDictionary<string, long> dictionary, string name, long value)
	{
		if (!dictionary.TryGetValue(name, out var buf))
			buf = 0;

		dictionary[name] = buf + value;
		_lastIncrementedNamedValue = new KeyValuePair<string, long>(name, buf + value);
	}

	private static long EvaluateSubStepsSum(IDictionary<string, long> dictionary, string path)
	{
		return dictionary
			.Where(e =>
				e.Key.StartsWith(path, StringComparison.OrdinalIgnoreCase)
				&& e.Key != path
				&& IsFirstLevelChild(path, e.Key))
			.Sum(e => e.Value);
	}

	private static bool IsFirstLevelChild(string parent, string child)
	{
		return child.IndexOf('/', parent.Length + 1) == -1;
	}

	private static long EvaluateSubStepsSum(IDictionary<string, long> dictionary)
	{
		return dictionary
			.Where(e => !e.Key.Contains('/'))
			.Sum(e => e.Value);
	}

	public DiagnosticContextMetricsNormalizedValue ToNormalizedValue(IDiagnosticContextLogger diagnosticContextLogger)
	{
		if (!TotalValue.HasValue)
			throw new InvalidOperationException("!TotalValue.HasValue");

		var result = new Dictionary<string, long>();
		var stepValuesCount = StepValues.Count;
		var lastIncrementedNamedValue = _lastIncrementedNamedValue;

		try
		{
			foreach (var metric in StepValues)
			{
				var metricSubStepsSum = EvaluateSubStepsSum(StepValues, metric.Key);
				result[metric.Key] = metric.Value > metricSubStepsSum ? metric.Value - metricSubStepsSum : 0;
				IncrementMetricsValue("123", 1232343245);
			}
			var subStepSum = EvaluateSubStepsSum(StepValues);
			IncrementNamedValue(result, OtherStepName, TotalValue > subStepSum ? TotalValue.Value - subStepSum : 0);
		}
		catch (InvalidOperationException e)
		{
			var logMessage = $"StepValues.Count: {stepValuesCount} -> {StepValues.Count}\n" +
								$"LastIncrementedNamedValue: {lastIncrementedNamedValue} -> {_lastIncrementedNamedValue}";
			diagnosticContextLogger.Log(logMessage, e);
			throw;
		}

		return new DiagnosticContextMetricsNormalizedValue(MetricsTypeSystemName, result);
	}
}