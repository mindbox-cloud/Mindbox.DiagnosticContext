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
/// Keeps one <see cref="TagKeys"/> instance per ordered list of tag names, so that the instance can be a part of
/// a cache key and finding it for a diagnostic context allocates nothing.
/// </summary>
internal sealed class TagKeysRegistry
{
	private readonly object _addLock = new();

	// Replaced as a whole on every addition, so reading needs no lock. Tag names come from the code of
	// the application, not from its traffic: the list stays short, and scanning it is cheaper than hashing the names.
	private volatile TagKeys[] _known = Array.Empty<TagKeys>();

	public TagKeys Get(Dictionary<string, string> tags)
	{
		if (tags.Count == 0)
			return TagKeys.None;

		return Find(_known, tags) ?? Add(tags);
	}

	private TagKeys Add(Dictionary<string, string> tags)
	{
		lock (_addLock)
		{
			var existing = Find(_known, tags);
			if (existing != null)
				return existing;

			var added = TagKeys.From(tags);

			var known = new TagKeys[_known.Length + 1];
			_known.CopyTo(known, 0);
			known[^1] = added;
			_known = known;

			return added;
		}
	}

	private static TagKeys? Find(TagKeys[] known, Dictionary<string, string> tags)
	{
		foreach (var tagKeys in known)
		{
			if (tagKeys.Matches(tags))
				return tagKeys;
		}

		return null;
	}
}
