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
