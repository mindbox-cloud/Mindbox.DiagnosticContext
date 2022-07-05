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

using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage;

public class MetricsAggregatedValue
{
	public MetricsAggregatedValue(MetricsType metricsType)
	{
		MetricsType = metricsType;
	}

	public MetricsType MetricsType { get; }

	public Dictionary<string, Int64ValueAggregator> StepValues { get; } = new Dictionary<string, Int64ValueAggregator>();
	public Int64ValueAggregator TotalValue { get; } = new Int64ValueAggregator();

	public void Add(DiagnosticContextMetricsNormalizedValue metricsNormalizedValue)
	{
		foreach (var step in metricsNormalizedValue.NormalizedValues)
		{
			if (!StepValues.TryGetValue(step.Key, out var aggregator))
			{
				aggregator = new Int64ValueAggregator();
				StepValues.Add(step.Key, aggregator);
			}
			aggregator.Add(step.Value);
		}
		TotalValue.Add(metricsNormalizedValue.NormalizedValues.Sum(step => step.Value));
	}
}