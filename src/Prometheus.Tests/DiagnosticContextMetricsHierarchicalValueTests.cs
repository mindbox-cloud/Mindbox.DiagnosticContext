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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mindbox.DiagnosticContext;
using Mindbox.DiagnosticContext.DynamicSteps;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Prometheus.Tests;

[TestClass]
public class DiagnosticContextMetricsHierarchicalValueTests
{
	[TestMethod]
	public void ToNormalizedValue_Tree()
	{
		AssertSameAsReference(100,
			("A", 50), ("A/B", 20), ("A/B/C", 5), ("A/D", 10), ("E", 30), ("E/F", 40));
	}

	[TestMethod]
	public void ToNormalizedValue_SiblingsSharingPrefix()
	{
		AssertSameAsReference(100,
			("Save", 30), ("SaveChanges", 10), ("Save/Inner", 5),
			("A/B", 20), ("A/BC", 7), ("A/B/C", 3), ("A", 40));
	}

	[TestMethod]
	public void ToNormalizedValue_PathsDifferingInCase()
	{
		AssertSameAsReference(100,
			("A", 50), ("a/B", 15), ("A/b/c", 3), ("a/B/c/D", 2), ("b", 10), ("B/a", 4));
	}

	[TestMethod]
	public void ToNormalizedValue_PathsEqualIgnoringCase()
	{
		AssertSameAsReference(100,
			("A", 50), ("a", 40), ("A/b", 10), ("a/B", 15));
	}

	[TestMethod]
	public void ToNormalizedValue_DeepPath()
	{
		AssertSameAsReference(100,
			("A", 90), ("A/B", 80), ("A/B/C", 70), ("A/B/C/D", 60), ("A/B/C/D/E", 50), ("A/B/C/D/E/F", 40), ("A/B/C/D/E/F/G", 1));
	}

	[TestMethod]
	public void ToNormalizedValue_NoSteps()
	{
		AssertSameAsReference(100);
	}

	[TestMethod]
	public void ToNormalizedValue_ChildrenGreaterThanParent()
	{
		AssertSameAsReference(10,
			("A", 5), ("A/B", 7), ("A/C", 3), ("D", 20));
	}

	[TestMethod]
	public void ToNormalizedValue_StepNamedOther()
	{
		AssertSameAsReference(100,
			("Other", 10), ("Other/A", 4), ("A", 20));
	}

	[TestMethod]
	public void ToNormalizedValue_NonAsciiPaths()
	{
		AssertSameAsReference(100,
			("Шаг", 50), ("ШАГ/Вложенный", 20), ("шаг/вложенный/ß", 5), ("Straße", 10), ("STRASSE/x", 3), ("ǅ", 4), ("ǆ/a", 1));
	}

	[TestMethod]
	public void ToNormalizedValue_RandomPaths()
	{
		var random = new Random(20261107);
		var segments = new[] { "a", "A", "ab", "Ab", "b", "B", "save", "Save", "SaveChanges", "_", "1", "z" };

		for (var iteration = 0; iteration < 2000; iteration++)
		{
			var allowPathsEqualIgnoringCase = iteration % 4 == 0;
			var steps = new Dictionary<string, long>();
			var stepsCount = random.Next(0, 60);
			for (var i = 0; i < stepsCount; i++)
			{
				var depth = random.Next(1, 6);
				var path = string.Join("/", Enumerable.Range(0, depth).Select(_ => segments[random.Next(segments.Length)]));
				if (!allowPathsEqualIgnoringCase
					&& steps.Keys.Any(key => key != path && string.Equals(key, path, StringComparison.OrdinalIgnoreCase)))
					continue;
				steps[path] = random.Next(0, 1000);
			}

			AssertSameAsReference(random.Next(0, 5000), steps.Select(s => (s.Key, s.Value)).ToArray());
		}
	}

	private static void AssertSameAsReference(long total, params (string Path, long Value)[] steps)
	{
		var hierarchicalValue = DiagnosticContextMetricsHierarchicalValue.FromMetricsType(
			WallClockTimeMetricsType.Create(new TestDateTimeAccessor(), "test"),
			new NullDiagnosticContextLogger());
		foreach (var (path, value) in steps)
			hierarchicalValue.IncrementMetricsValue(path, TimeSpan.FromMilliseconds(value).Ticks);
		hierarchicalValue.SetTotal(TimeSpan.FromMilliseconds(total).Ticks);

		Dictionary<string, long> expected;
		try
		{
			expected = EvaluateReferenceNormalizedValues(steps.ToDictionary(s => s.Path, s => s.Value), total);
		}
		catch (ArgumentOutOfRangeException)
		{
			// Paths equal ignoring case make the original algorithm look for '/' beyond the end of the path.
			Assert.ThrowsException<ArgumentOutOfRangeException>(hierarchicalValue.ToNormalizedValue);
			return;
		}

		var actual = hierarchicalValue.ToNormalizedValue().NormalizedValues;

		CollectionAssert.AreEqual(
			expected.ToList(),
			actual.ToList(),
			$"Steps: {string.Join(", ", steps.Select(s => $"{s.Path}={s.Value}"))}");
	}

	// The original quadratic algorithm, kept as the reference for metric values.
	private static Dictionary<string, long> EvaluateReferenceNormalizedValues(Dictionary<string, long> steps, long total)
	{
		var result = new Dictionary<string, long>();
		foreach (var step in steps)
		{
			var subStepsSum = steps
				.Where(e =>
					e.Key.StartsWith(step.Key, StringComparison.OrdinalIgnoreCase)
					&& e.Key != step.Key
					&& e.Key.IndexOf('/', step.Key.Length + 1) == -1)
				.Sum(e => e.Value);
			result[step.Key] = step.Value > subStepsSum ? step.Value - subStepsSum : 0;
		}

		var topLevelSum = steps.Where(e => !e.Key.Contains('/')).Sum(e => e.Value);
		result.TryGetValue("Other", out var other);
		result["Other"] = other + (total > topLevelSum ? total - topLevelSum : 0);
		return result;
	}
}
