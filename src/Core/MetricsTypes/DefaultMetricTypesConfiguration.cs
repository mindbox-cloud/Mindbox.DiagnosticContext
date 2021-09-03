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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	public sealed class DefaultMetricTypesConfiguration
	{
		private readonly ICurrentTimeAccessor currentTimeAccessor;
		private const string CpuTimeMetricsSystemName = "ProcessingCpuTime";
		private const string WallClockTimeMetricsSystemName = "ProcessingTime";
		private const string ThreadAllocatedBytesSystemName = "ThreadAllocatedBytes";

		public DefaultMetricTypesConfiguration(ICurrentTimeAccessor? currentTimeAccessor = null)
		{
			this.currentTimeAccessor = currentTimeAccessor ?? new DefaultCurrentTimeAccessor();
		}

		public MetricsTypeCollection GetStandardMetricsTypes()
		{
			return GetStandardMetricsTypes(NullMetricsMeasurerCreationHandler.Instance);
		}

		public MetricsTypeCollection GetAsyncMetricsTypes()
		{
			return new(new MetricsType[]
			{
				WallClockTimeMetricsType.Create(currentTimeAccessor, WallClockTimeMetricsSystemName)
			});
		}

		public MetricsTypeCollection GetMetricTypesWithThreadAllocatedBytes()
		{
			return new(new MetricsType[]
			{
				WallClockTimeMetricsType.Create(currentTimeAccessor, WallClockTimeMetricsSystemName),
				ThreadAllocatedBytesMetricsType.Create(currentTimeAccessor, ThreadAllocatedBytesSystemName)
			});
		}

		public MetricsTypeCollection GetMetricsTypesWithCpuTimeTracking(string featureSystemName)
		{
			return GetStandardMetricsTypes(NullMetricsMeasurerCreationHandler.Instance);
		}

		private MetricsTypeCollection GetStandardMetricsTypes(IMetricsMeasurerCreationHandler handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var metricsTypes = new List<MetricsType>
			{
				WallClockTimeMetricsType.Create(currentTimeAccessor, WallClockTimeMetricsSystemName),
				ThreadAllocatedBytesMetricsType.Create(currentTimeAccessor, ThreadAllocatedBytesSystemName)
			};
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// CPU metrics type uses WinAPI in CpuTimeMeasurer
				metricsTypes.Add(CpuTimeMetricsType.Create(currentTimeAccessor, CpuTimeMetricsSystemName, handler));
			}

			return new MetricsTypeCollection(metricsTypes);
		}
	}
}
