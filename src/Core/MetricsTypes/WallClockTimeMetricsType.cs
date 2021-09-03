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

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	internal class WallClockTimeMetricsType : MetricsType<WallClockTimeMeasurer>
	{
		public static WallClockTimeMetricsType Create(ICurrentTimeAccessor currentTimeAccessor, string systemName)
		{
			return new WallClockTimeMetricsType(currentTimeAccessor, systemName);
		}

		public override string Units => "[ms]";

		public override long ConvertMetricValue(long rawMetricValue) =>
			(long)TimeSpan.FromTicks(rawMetricValue).TotalMilliseconds;

		private WallClockTimeMetricsType(ICurrentTimeAccessor currentTimeAccessor, string systemName) : base(currentTimeAccessor, systemName, NullMetricsMeasurerCreationHandler.Instance)
		{
		}

		protected override WallClockTimeMeasurer CreateMeasurerCore()
		{
			return new WallClockTimeMeasurer(CurrentTimeAccessor, SystemName);
		}
	}
}
