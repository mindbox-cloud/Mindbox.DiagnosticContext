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
using System.Runtime.InteropServices;

namespace Mindbox.DiagnosticContext.MetricsTypes;

public sealed class DefaultMetricTypesConfiguration
{
	private readonly ICurrentTimeAccessor _currentTimeAccessor;
	private const string CpuTimeMetricsSystemName = "ProcessingCpuTime";
	private const string WallClockTimeMetricsSystemName = "ProcessingTime";

	public DefaultMetricTypesConfiguration(ICurrentTimeAccessor? currentTimeAccessor = null)
	{
		_currentTimeAccessor = currentTimeAccessor ?? new DefaultCurrentTimeAccessor();
	}

	public MetricsTypeCollection GetDefaultMetricsTypes()
	{
		var metricsTypes = new List<MetricsType>
		{
			WallClockTimeMetricsType.Create(_currentTimeAccessor, WallClockTimeMetricsSystemName),
		};

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			// CPU metrics type uses WinAPI in CpuTimeMeasurer
			metricsTypes.Add(CpuTimeMetricsType.Create(_currentTimeAccessor, CpuTimeMetricsSystemName));
		}

		return new MetricsTypeCollection(metricsTypes);
	}
}