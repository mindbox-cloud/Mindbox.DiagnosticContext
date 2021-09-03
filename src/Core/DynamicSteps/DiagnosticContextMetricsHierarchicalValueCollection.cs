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

namespace Mindbox.DiagnosticContext.DynamicSteps
{
	internal class DiagnosticContextMetricsHierarchicalValueCollection
	{
		public static DiagnosticContextMetricsHierarchicalValueCollection FromMetricsTypeCollection(
			MetricsTypeCollection collection)
		{
			return new DiagnosticContextMetricsHierarchicalValueCollection(
				collection.MetricsTypes.Select(mt => DiagnosticContextMetricsHierarchicalValue.FromMetricsType(mt)));
		}

		private readonly IDictionary<string, DiagnosticContextMetricsHierarchicalValue> metricTypeSystemNameToValuesMapping;

		private DiagnosticContextMetricsHierarchicalValueCollection(
			IEnumerable<DiagnosticContextMetricsHierarchicalValue> metricsValues)
		{
			if (metricsValues == null)
				throw new ArgumentNullException(nameof(metricsValues));

			metricTypeSystemNameToValuesMapping = metricsValues.ToDictionary(v => v.MetricsTypeSystemName);
		}

		public void SetStepMetricsValues(string currentStep, MetricsMeasurerCollection measurerCollection)
		{
			foreach (var measurer in measurerCollection.Measurers)
			{
				var systemName = measurer.MetricsTypeSystemName;
				if (!metricTypeSystemNameToValuesMapping.ContainsKey(systemName))
					throw new InvalidOperationException(
						$"{nameof(metricTypeSystemNameToValuesMapping)} does not contain {systemName}");

				metricTypeSystemNameToValuesMapping[systemName].IncrementMetricsValue(
					currentStep,
					measurer.GetValue() ?? 0);
			}
		}

		public void SetTotal(MetricsMeasurerCollection measurerCollection)
		{
			foreach (var measurer in measurerCollection.Measurers)
			{
				var systemName = measurer.MetricsTypeSystemName;
				if (!metricTypeSystemNameToValuesMapping.ContainsKey(systemName))
					throw new InvalidOperationException(
						$"{nameof(metricTypeSystemNameToValuesMapping)} does not contain {systemName}");

				metricTypeSystemNameToValuesMapping[systemName].SetTotal(measurer.GetValue() ?? 0);
			}
		}

		public DiagnosticContextMetricsNormalizedValueCollection ToNormalizedValueCollection()
		{
			return new DiagnosticContextMetricsNormalizedValueCollection(
				metricTypeSystemNameToValuesMapping.Values.Select(metricsValue => metricsValue.ToNormalizedValue()));
		}
	}
}
