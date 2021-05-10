#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itc.Commons.Model
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
