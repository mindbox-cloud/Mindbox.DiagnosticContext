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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mindbox.DiagnosticContext.Prometheus;

namespace Prometheus.Tests;

[TestClass]
public class TagKeysRegistryTests
{
	private readonly TagKeysRegistry _registry = new();

	[TestMethod]
	public void Get_NoTags_ReturnsNone()
	{
		var tagKeys = _registry.Get(new Dictionary<string, string>());

		Assert.AreSame(TagKeys.None, tagKeys);
		Assert.AreEqual(0, tagKeys.Names.Count);
	}

	[TestMethod]
	public void Get_SameNamesWithOtherValues_ReturnsSameInstance()
	{
		var first = _registry.Get(new Dictionary<string, string> { ["firstTag"] = "1", ["secondTag"] = "2" });
		var second = _registry.Get(new Dictionary<string, string> { ["firstTag"] = "3", ["secondTag"] = "4" });

		Assert.AreSame(first, second);
		CollectionAssert.AreEqual(new[] { "firstTag", "secondTag" }, first.Names.ToArray());
	}

	[TestMethod]
	public void Get_SameNamesInOtherOrder_ReturnsOtherInstance()
	{
		var first = _registry.Get(new Dictionary<string, string> { ["firstTag"] = "1", ["secondTag"] = "2" });
		var second = _registry.Get(new Dictionary<string, string> { ["secondTag"] = "2", ["firstTag"] = "1" });

		Assert.AreNotSame(first, second);
		CollectionAssert.AreEqual(new[] { "secondTag", "firstTag" }, second.Names.ToArray());
	}

	[TestMethod]
	public void Get_OtherNames_ReturnsOtherInstance()
	{
		var first = _registry.Get(new Dictionary<string, string> { ["firstTag"] = "1" });
		var second = _registry.Get(new Dictionary<string, string> { ["secondTag"] = "1" });
		var both = _registry.Get(new Dictionary<string, string> { ["firstTag"] = "1", ["secondTag"] = "1" });

		Assert.AreNotSame(first, second);
		Assert.AreNotSame(first, both);
		Assert.AreSame(first, _registry.Get(new Dictionary<string, string> { ["firstTag"] = "2" }));
		Assert.AreSame(second, _registry.Get(new Dictionary<string, string> { ["secondTag"] = "2" }));
	}

	[TestMethod]
	public void Get_TagOverwritten_KeepsItsPlace()
	{
		var tags = new Dictionary<string, string> { ["firstTag"] = "1", ["secondTag"] = "2" };
		var before = _registry.Get(tags);

		tags["firstTag"] = "3";

		Assert.AreSame(before, _registry.Get(tags));
	}
}
