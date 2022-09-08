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

namespace Mindbox.DiagnosticContext;

internal class DiagnosticContextCollection : IDisposable
{
	private readonly List<IDiagnosticContext> _linkedContexts = new();

	internal void LinkDiagnosticContext(IDiagnosticContext context)
	{
		_linkedContexts.Add(context);
	}

	public IDisposable Measure(string stepName)
	{
		return new DisposableContainer(
			_linkedContexts
				.Where(linkedContext => !linkedContext.IsDisposed)
				.Select(context => context.Measure(stepName))
				.ToArray());
	}

	public void Increment(string counterPath)
	{
		foreach (var context in _linkedContexts)
			context.Increment(counterPath);
	}

	public void Dispose()
	{
		foreach (var linkedContext in _linkedContexts)
			linkedContext.Dispose();
	}
}