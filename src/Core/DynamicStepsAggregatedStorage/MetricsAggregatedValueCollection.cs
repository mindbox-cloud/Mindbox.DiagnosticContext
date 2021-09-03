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
using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.MetricItem;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage
{
	public sealed class MetricsAggregatedValueCollection
	{
		private readonly ICollection<MetricsAggregatedValue> aggregatedValues;

		public MetricsAggregatedValueCollection(MetricsTypeCollection metricsTypes)
		{
			if (metricsTypes == null)
				throw new ArgumentNullException(nameof(metricsTypes));

			aggregatedValues = metricsTypes.MetricsTypes.Select(mt => new MetricsAggregatedValue(mt)).ToList();
		}

		public void Add(DiagnosticContextMetricsNormalizedValueCollection normalizedMetricsValues)
		{
			foreach (var metricsAggregatedValue in aggregatedValues)
			{
				var metricsNormalizedValue = normalizedMetricsValues.GetValueByMetricsTypeSystemName(
					metricsAggregatedValue.MetricsType.SystemName);

				metricsAggregatedValue.Add(metricsNormalizedValue);
			}
		}

		public IEnumerable<MetricsAggregatedValue> GetMetricsAggregatedValues()
		{
			return aggregatedValues;
		}
	}
}
