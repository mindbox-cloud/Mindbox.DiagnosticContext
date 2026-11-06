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
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.DynamicSteps;

internal class DiagnosticContextMetricsHierarchicalValue
{
	public static DiagnosticContextMetricsHierarchicalValue FromMetricsType(
		MetricsType metricsType,
		IDiagnosticContextLogger diagnosticContextLogger)
	{
		return new DiagnosticContextMetricsHierarchicalValue(metricsType, diagnosticContextLogger);
	}

	private const string OtherStepName = "Other";

	private IDictionary<string, long> StepValues { get; } = new Dictionary<string, long>();
	private long? TotalValue { get; set; }

	public string MetricsTypeSystemName => _metricsType.SystemName;

	private bool _isDisposing;
	private readonly MetricsType _metricsType;
#pragma warning disable IDE0052
	private readonly IDiagnosticContextLogger _diagnosticContextLogger;
#pragma warning restore IDE0052

	private DiagnosticContextMetricsHierarchicalValue(MetricsType metricsType, IDiagnosticContextLogger diagnosticContextLogger)
	{
		_metricsType = metricsType;
		_diagnosticContextLogger = diagnosticContextLogger;
		_isDisposing = false;
	}

	public void SetTotal(long total)
	{
		if (TotalValue.HasValue)
			throw new InvalidOperationException("TotalValue.HasValue");

		TotalValue = _metricsType.ConvertMetricValue(total);
	}

	public void IncrementMetricsValue(string path, long increment)
	{
		if (_isDisposing)
		{
			var stackTrace = new StackTrace(true).ToString();
			_diagnosticContextLogger.Log(
				$"A new metric ({path} - {increment}) added while disposing.",
				logLevel: LogLevel.Warning,
				additionalProperties: new Dictionary<string, object> { { "stackTrace", stackTrace } });
		}

		IncrementNamedValue(StepValues, path, _metricsType.ConvertMetricValue(increment));
	}

	private static void IncrementNamedValue(IDictionary<string, long> dictionary, string name, long value)
	{
		if (!dictionary.TryGetValue(name, out var buf))
			buf = 0;

		dictionary[name] = buf + value;
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

	// Same result as EvaluateSubStepsSum(dictionary, path) for every path, without comparing each path with all others.
	// After sorting with OrdinalIgnoreCase, all paths starting with a path follow it as one range.
	// A path equal to another one ignoring case may sort before it, but IsFirstLevelChild throws for such a pair
	// when the earlier one is evaluated, as it does in the per-path evaluation.
	// The range is proven contiguous only for ASCII, so callers fall back to the per-path evaluation for other paths.
	private static Dictionary<string, long> EvaluateSubStepsSums(IDictionary<string, long> dictionary)
	{
		var paths = new string[dictionary.Count];
		var values = new long[dictionary.Count];
		var index = 0;
		foreach (var step in dictionary)
		{
			paths[index] = step.Key;
			values[index] = step.Value;
			index++;
		}
		Array.Sort(paths, values, StringComparer.OrdinalIgnoreCase);

		var result = new Dictionary<string, long>(paths.Length);
		for (var i = 0; i < paths.Length; i++)
		{
			var path = paths[i];
			long sum = 0;
			for (var j = i + 1; j < paths.Length && paths[j].StartsWith(path, StringComparison.OrdinalIgnoreCase); j++)
			{
				if (IsFirstLevelChild(path, paths[j]))
					sum = checked(sum + values[j]);
			}

			result[path] = sum;
		}

		return result;
	}

	private static bool AreAllPathsAscii(IEnumerable<string> paths)
	{
		return paths.All(path => path.All(char.IsAscii));
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

	public DiagnosticContextMetricsNormalizedValue ToNormalizedValue()
	{
		if (!TotalValue.HasValue)
			throw new InvalidOperationException("!TotalValue.HasValue");

		_isDisposing = true;

		var result = new Dictionary<string, long>();
		var subStepsSums = AreAllPathsAscii(StepValues.Keys) ? EvaluateSubStepsSums(StepValues) : null;

		foreach (var metric in StepValues)
		{
			var metricSubStepsSum = subStepsSums?[metric.Key] ?? EvaluateSubStepsSum(StepValues, metric.Key);
			result[metric.Key] = metric.Value > metricSubStepsSum ? metric.Value - metricSubStepsSum : 0;
		}
		var subStepSum = EvaluateSubStepsSum(StepValues);
		IncrementNamedValue(result, OtherStepName, TotalValue > subStepSum ? TotalValue.Value - subStepSum : 0);

		return new DiagnosticContextMetricsNormalizedValue(MetricsTypeSystemName, result);
	}
}