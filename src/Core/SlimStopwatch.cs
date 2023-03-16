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
/// We don't need to have high-resoultion here, so we use DateTime and ticks as a measurement.
/// </summary>
internal class SlimStopwatch
{
	private readonly ICurrentTimeAccessor _currentTimeAccessor;

	private DateTime _lastStartTimeUtc;
	private TimeSpan _lastStopElapsed;


	public SlimStopwatch(ICurrentTimeAccessor currentTimeAccessor)
	{
		_currentTimeAccessor = currentTimeAccessor;
	}


	public TimeSpan Elapsed => IsRunning ? _lastStopElapsed + ElapsedSinceLastStart : _lastStopElapsed;

	public bool IsRunning { get; private set; }


	private TimeSpan ElapsedSinceLastStart
	{
		get
		{
			var result = _currentTimeAccessor.CurrentDateTimeUtc - _lastStartTimeUtc;
			return result < TimeSpan.Zero ? TimeSpan.Zero : result;
		}
	}

	public void Start()
	{
		if (!IsRunning)
		{
			IsRunning = true;

			_lastStartTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc;
		}
	}

	public void Stop()
	{
		if (IsRunning)
		{
			_lastStopElapsed += ElapsedSinceLastStart;
			IsRunning = false;
		}
	}
}