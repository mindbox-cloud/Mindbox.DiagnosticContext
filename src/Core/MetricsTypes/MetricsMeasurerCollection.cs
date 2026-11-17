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

/// <summary>
/// The measurers of one step. A struct over an array: the step measurement allocates the array, this adds no object.
/// </summary>
internal readonly struct MetricsMeasurerCollection
{
	private static readonly SafeExceptionHandler _handler = new();

	public MetricsMeasurerCollection(MetricsMeasurer[] measurers)
	{
		Measurers = measurers ?? throw new ArgumentNullException(nameof(measurers));
	}

	public MetricsMeasurer[] Measurers { get; }

	public void Start()
	{
		foreach (var metricsMeasurer in Measurers)
		{
			try
			{
				metricsMeasurer.Start();
			}
			catch (Exception exception)
			{
				_handler.ProcessException(exception);
			}
		}
	}

	public void Stop()
	{
		foreach (var metricsMeasurer in Measurers)
		{
			try
			{
				metricsMeasurer.Stop();
			}
			catch (Exception exception)
			{
				_handler.ProcessException(exception);
			}
		}
	}
}
