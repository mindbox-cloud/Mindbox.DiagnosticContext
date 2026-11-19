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
/// The tag names of a diagnostic context, in the order the tags were set. They are the label names of the metrics
/// the context publishes, so contexts can reuse a metric handle only when they share the same instance.
/// </summary>
internal sealed class TagKeys
{
	public static readonly TagKeys None = new(Array.Empty<string>());

	private readonly string[] _names;

	private TagKeys(string[] names)
	{
		_names = names;
	}

	public IReadOnlyList<string> Names => _names;

	public static TagKeys From(Dictionary<string, string> tags)
	{
		var names = new string[tags.Count];
		tags.Keys.CopyTo(names, 0);

		return new TagKeys(names);
	}

	public bool Matches(Dictionary<string, string> tags)
	{
		if (tags.Count != _names.Length)
			return false;

		var index = 0;
		foreach (var tag in tags)
		{
			if (tag.Key != _names[index++])
				return false;
		}

		return true;
	}
}
