#nullable disable

using System;
using Mindbox.DiagnosticContext.MetricItem;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage
{
	public class DiagnosticContextDynamicStepsAggregatedStorage
	{
		public MetricsAggregatedValueCollection MetricsAggregatedValues { get; }
		public long ItemsCount { get; private set; }

		public DiagnosticContextDynamicStepsAggregatedStorage(MetricsTypeCollection metricsTypes)
		{
			MetricsAggregatedValues = new MetricsAggregatedValueCollection(metricsTypes);
		}

		public void CollectItemData(DiagnosticContextMetricsItem item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			ItemsCount++;

			var normalizedMetricsValues = item.GetNormalizedMetricsValues();
			MetricsAggregatedValues.Add(normalizedMetricsValues);
		}
	}
}
