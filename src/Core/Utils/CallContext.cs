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

using System.Collections.Concurrent;
using System.Threading;

namespace Mindbox.DiagnosticContext;

public static class CallContext
{
	private static readonly ConcurrentDictionary<string, AsyncLocal<object?>> _state = new();

	public static void LogicalSetData(string name, object? data) =>
		_state.GetOrAdd(name, _ => new AsyncLocal<object?>()).Value = data;

	public static object? LogicalGetData(string name) =>
		_state.TryGetValue(name, out var data) ? data.Value : null;

	public static void FreeNamedDataSlot(string name)
	{
		_state.GetOrAdd(name, _ => new AsyncLocal<object?>()).Value = null;
	}
}