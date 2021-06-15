#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mindbox.DiagnosticContext.MetricItem
{
	public class DiagnosticContextMetricsNormalizedValueCollection
	{
		private readonly Dictionary<string, DiagnosticContextMetricsNormalizedValue> diagnosticContextMetricsNormalizedValues;

		public DiagnosticContextMetricsNormalizedValueCollection(
			IEnumerable<DiagnosticContextMetricsNormalizedValue> normalizedValues)
		{
			diagnosticContextMetricsNormalizedValues = normalizedValues.ToDictionary(v => v.MetricTypeSystemName);
		}

		public DiagnosticContextMetricsNormalizedValue GetValueByMetricsTypeSystemName(string metricsTypeSystemName)
		{
			if (!diagnosticContextMetricsNormalizedValues.ContainsKey(metricsTypeSystemName))
				throw new InvalidOperationException($"{metricsTypeSystemName} metrics not found");

			return diagnosticContextMetricsNormalizedValues[metricsTypeSystemName];
		}
	}
}
