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

namespace Mindbox.DiagnosticContext.MetricsTypes;

public class DiagnosticContextInternalProcessingTimeMeasurer
{
	private readonly WallClockTimeMeasurer _wallClockTimeMeasurer;

	private readonly SafeExceptionHandler _safeExceptionHandler;

	private const string InternalProcessingTime = "InternalProcessingTime";

	public DiagnosticContextInternalProcessingTimeMeasurer(IDiagnosticContextLogger logger)
	{
		_safeExceptionHandler = new SafeExceptionHandler(logger);
		_wallClockTimeMeasurer = new WallClockTimeMeasurer(new DefaultCurrentTimeAccessor(), InternalProcessingTime);
	}

	public void Measure(Action action)
	{
		_safeExceptionHandler.HandleExceptions(() => _wallClockTimeMeasurer.Start());
		action();
		_safeExceptionHandler.HandleExceptions(() => _wallClockTimeMeasurer.Stop());
	}

	public long Elapsed => _wallClockTimeMeasurer.GetValue() ?? 0;
	public string MetricTypeSystemName => _wallClockTimeMeasurer.MetricsTypeSystemName;
}