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

namespace Mindbox.DiagnosticContext
{
	public abstract class MetricsType
	{
		protected ICurrentTimeAccessor CurrentTimeAccessor { get; }

		internal MetricsType(ICurrentTimeAccessor currentTimeAccessor, string systemName)
		{
			if (string.IsNullOrEmpty(systemName))
				throw new ArgumentNullException(nameof(systemName));
			this.CurrentTimeAccessor = currentTimeAccessor;

			SystemName = systemName;
		}

		public string SystemName { get; }
		public virtual string Units => string.Empty;

		public virtual long ConvertMetricValue(long rawMetricValue) => rawMetricValue;
		internal abstract MetricsMeasurer CreateMeasurer();
	}

	internal abstract class MetricsType<TMetricMeasurer> : MetricsType
		where TMetricMeasurer : MetricsMeasurer
	{
		private readonly IMetricsMeasurerCreationHandler handler;

		protected MetricsType(ICurrentTimeAccessor currentTimeAccessor, string systemName, IMetricsMeasurerCreationHandler handler) 
			: base(currentTimeAccessor, systemName)
		{
			this.handler = handler;
		}

		protected abstract TMetricMeasurer CreateMeasurerCore();

		internal override MetricsMeasurer CreateMeasurer()
		{
			var measurer = CreateMeasurerCore();
			handler.HandleMeasurerCreation(measurer);
			return measurer;
		}
	}
}
