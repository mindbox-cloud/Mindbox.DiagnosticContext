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

namespace Mindbox.DiagnosticContext.Prometheus;

/// <summary>
/// Builds the label values of a metric from the tags of a diagnostic context, in the order of <see cref="TagKeys"/>.
/// <para>
/// prometheus-net does not copy the array it receives: the array becomes the key of the time series it creates.
/// An array must never change after it was passed to a counter, so a reusable buffer cannot be used here.
/// </para>
/// </summary>
internal static class LabelValues
{
	public static string[] From(Dictionary<string, string> tags)
	{
		if (tags.Count == 0)
			return Array.Empty<string>();

		var values = new string[tags.Count];
		CopyTagValues(tags, values, 0);

		return values;
	}

	public static string[] Prepend(string leadingValue, Dictionary<string, string> tags)
	{
		var values = new string[tags.Count + 1];
		values[0] = leadingValue;
		CopyTagValues(tags, values, 1);

		return values;
	}

	public static string[] Append(Dictionary<string, string> tags, string firstTrailingValue, string secondTrailingValue)
	{
		var values = new string[tags.Count + 2];
		CopyTagValues(tags, values, 0);
		values[^2] = firstTrailingValue;
		values[^1] = secondTrailingValue;

		return values;
	}

	// Not tags.Values.CopyTo: the first use of Values allocates a collection object in every dictionary.
	private static void CopyTagValues(Dictionary<string, string> tags, string[] destination, int index)
	{
		foreach (var tag in tags)
			destination[index++] = tag.Value;
	}
}
