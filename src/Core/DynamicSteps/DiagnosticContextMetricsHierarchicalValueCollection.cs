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
