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
using System.Diagnostics;

namespace Mindbox.DiagnosticContext.MetricsTypes;

public class DiagnosticContextInternalProcessingTimeMeasurer
{
	private const string InternalProcessingTime = "InternalProcessingTime";
	private bool _isFinished;
	private long _elapsed;

	public void Measure(Action action)
	{
		if (_isFinished)
			throw new InvalidOperationException("Cannot use one measurer twice");

		var stopwatch = Stopwatch.StartNew();
		action();
		stopwatch.Stop();

		Elapsed = stopwatch.ElapsedMilliseconds;
	}

	public long Elapsed
	{
		get => _elapsed;
		private set
		{
			_elapsed = value;
			_isFinished = true;
		}
	}

	public string MetricTypeSystemName => InternalProcessingTime;
}