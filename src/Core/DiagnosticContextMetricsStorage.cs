#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itc.Commons.Model
{
	public class DiagnosticContextMetricsStorage : NewRelicMetricsStorage<DiagnosticContextMetricsItem>
	{
		private readonly Dictionary<string, DiagnosticContextDynamicStepsAggregatedStorage> dynamicStepsPerMetricPrefix 
			= new Dictionary<string, DiagnosticContextDynamicStepsAggregatedStorage>();

		private readonly Dictionary<string, DiagnosticContextCountersStorage> countersPerMetricsPrefix
			= new Dictionary<string, DiagnosticContextCountersStorage>();

		private readonly Dictionary<string, DiagnosticContextReportedValuesStorage> reportedValuesPerMetricsPrefix
			= new Dictionary<string, DiagnosticContextReportedValuesStorage>();

		public IReadOnlyDictionary<string, DiagnosticContextCountersStorage> CountersPerMetricsPrefix 
			=> countersPerMetricsPrefix;

		public IReadOnlyDictionary<string, DiagnosticContextReportedValuesStorage> ReportedValuesPerMetricsPrefix 
			=> reportedValuesPerMetricsPrefix;

		public IReadOnlyDictionary<string, DiagnosticContextDynamicStepsAggregatedStorage> DynamicStepsPerMetricPrefix 
			=> dynamicStepsPerMetricPrefix;

		public override bool HasData => DynamicStepsPerMetricPrefix.Any() 
			|| CountersPerMetricsPrefix.Any() 
			|| ReportedValuesPerMetricsPrefix.Any();

		public override void CollectItemData(DiagnosticContextMetricsItem item)
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
