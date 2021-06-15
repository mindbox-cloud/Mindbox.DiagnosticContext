#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage;
using Mindbox.DiagnosticContext.MetricItem;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext
{
	public class DiagnosticContextMetricsStorage
	{
		private readonly Dictionary<string, DiagnosticContextDynamicStepsAggregatedStorage> dynamicStepsPerMetricPrefix = new();

		private readonly Dictionary<string, DiagnosticContextCountersStorage> countersPerMetricsPrefix = new();

		private readonly Dictionary<string, DiagnosticContextReportedValuesStorage> reportedValuesPerMetricsPrefix = new();

		public IReadOnlyDictionary<string, DiagnosticContextCountersStorage> CountersPerMetricsPrefix => countersPerMetricsPrefix;

		public IReadOnlyDictionary<string, DiagnosticContextReportedValuesStorage> ReportedValuesPerMetricsPrefix
			=> reportedValuesPerMetricsPrefix;

		public IReadOnlyDictionary<string, DiagnosticContextDynamicStepsAggregatedStorage> DynamicStepsPerMetricPrefix 
			=> dynamicStepsPerMetricPrefix;

		public bool HasData => DynamicStepsPerMetricPrefix.Any() 
			|| CountersPerMetricsPrefix.Any() 
			|| ReportedValuesPerMetricsPrefix.Any();

		public void CollectItemData(DiagnosticContextMetricsItem item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			GetStorageForMetricPrefix(item.MetricsTypes, item.MetricPrefix).CollectItemData(item);

			GetCounterStorageForMetricPrefix(item.MetricPrefix).CollectItemData(item.Counters);

			GetReportedValuesStorageForMetricPrefix(item.MetricPrefix).CollectItemData(item.ReportedValues);
		}
		
		private DiagnosticContextCountersStorage GetCounterStorageForMetricPrefix(string prefix)
		{
			if (!countersPerMetricsPrefix.ContainsKey(prefix))
			{
				countersPerMetricsPrefix.Add(prefix, new DiagnosticContextCountersStorage());
			}
			return countersPerMetricsPrefix[prefix];
		}

		private DiagnosticContextDynamicStepsAggregatedStorage GetStorageForMetricPrefix(
			MetricsTypeCollection metricsTypes, string prefix)
		{
			if(!dynamicStepsPerMetricPrefix.ContainsKey(prefix))
			{
				dynamicStepsPerMetricPrefix.Add(prefix, new DiagnosticContextDynamicStepsAggregatedStorage(metricsTypes));
			}

			return dynamicStepsPerMetricPrefix[prefix];
		}

		private DiagnosticContextReportedValuesStorage GetReportedValuesStorageForMetricPrefix(string prefix)
		{
			if (!reportedValuesPerMetricsPrefix.ContainsKey(prefix))
			{
				reportedValuesPerMetricsPrefix.Add(prefix, new DiagnosticContextReportedValuesStorage());
			}

			return reportedValuesPerMetricsPrefix[prefix];
		}
	}
}
