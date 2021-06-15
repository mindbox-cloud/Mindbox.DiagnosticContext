#nullable disable

using System;
using System.Collections.Generic;
using Mindbox.DiagnosticContext.DynamicSteps;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext.MetricItem
{
	public class DiagnosticContextMetricsItem
	{
		private DiagnosticContextMetricsNormalizedValueCollection normalizedMetricsValues;

		public DiagnosticContextMetricsItem(
			MetricsTypeCollection metricsTypes,
			string metricPrefix)
		{
			MetricsTypes = metricsTypes;
			MetricPrefix = metricPrefix;

			DynamicSteps = new DiagnosticContextDynamicSteps(metricsTypes);
		}

		internal void PrepareForCollection()
		{
			normalizedMetricsValues = DynamicSteps.GetNormalizedMetricsValues();
		}

		public DiagnosticContextMetricsNormalizedValueCollection GetNormalizedMetricsValues()
		{
			if (normalizedMetricsValues == null)
				throw new InvalidOperationException($"Metrics hasn't been collected yet");

			return normalizedMetricsValues;
		}

		public MetricsTypeCollection MetricsTypes { get; }
		public string MetricPrefix { get; }
		public bool IsEmpty => false;

		public DiagnosticContextDynamicSteps DynamicSteps { get; }
		
		public Dictionary<string, int> Counters { get; } = new Dictionary<string, int>();
		public Dictionary<string, long> ReportedValues { get; } = new Dictionary<string, long>();
	}
}
