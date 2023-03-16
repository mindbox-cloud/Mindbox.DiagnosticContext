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

namespace Mindbox.DiagnosticContext.MetricsTypes;

internal sealed class WallClockTimeMeasurer : MetricsMeasurer
{
	private ReliableStopwatch _stopwatch;

	public WallClockTimeMeasurer(ICurrentTimeAccessor currentTimeAccessor, string metricsTypeSystemName)
		: base(currentTimeAccessor, metricsTypeSystemName)
	{
	}

	protected override long? GetValueCore()
	{
		return _stopwatch.Elapsed.Ticks;
	}

	protected override void StartCore()
	{
		_stopwatch = new ReliableStopwatch(CurrentTimeAccessor);
		_stopwatch.Start();
	}

	protected override void StopCore()
	{
		if (_stopwatch == null)
			throw new InvalidOperationException("stopwatch == null");

		_stopwatch.Stop();
	}
}