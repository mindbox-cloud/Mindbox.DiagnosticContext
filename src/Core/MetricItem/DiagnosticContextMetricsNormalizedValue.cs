#nullable disable

using System.Collections.Generic;

namespace Itc.Commons
{
	public class DiagnosticContextMetricsNormalizedValue
	{
		public string MetricTypeSystemName { get; }
		public IReadOnlyDictionary<string, long> NormalizedValues { get; }

		public DiagnosticContextMetricsNormalizedValue(
			string metricTypeSystemName, Dictionary<string, long> normalizedValues)
		{
			MetricTypeSystemName = metricTypeSystemName;
			NormalizedValues = normalizedValues;
		}
	}
}
