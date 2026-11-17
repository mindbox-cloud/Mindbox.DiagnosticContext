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

namespace Mindbox.DiagnosticContext;

/// <summary>
/// One <see cref="DiagnosticContext.Measure"/> call: the step timer of this context plus the measurements of
/// the contexts linked into it. Replaces a container, an array of items and a tags adapter on the hot path.
/// </summary>
internal sealed class StepMeasurement : IMeasurement
{
	private readonly IDisposable _additionalMetrics;
	private readonly IDisposable _stepTimer;

	private bool _disposed;

	public StepMeasurement(IDisposable additionalMetrics, IDisposable stepTimer)
	{
		_additionalMetrics = additionalMetrics;
		_stepTimer = stepTimer;
	}

	public IMeasurement SetSpanAttribute(string attribute, string value)
	{
		return this;
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		// Same order as the container this replaced: linked contexts first, then the step of this context.
		_additionalMetrics?.Dispose();
		_stepTimer?.Dispose();

		// Only now: a throwing linked measurement leaves this one retryable, as the container was.
		_disposed = true;
	}
}
