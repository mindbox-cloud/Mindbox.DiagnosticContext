using System;
using System.Collections.Generic;

using Itc.Commons;
using Itc.Commons.Model;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class PrometheusDiagnosticContextMetricsCollection : IDiagnosticContextMetricsCollection
	{	
		private readonly Dictionary<string, DiagnosticContextMetricsStorage> storagesByMetricPrefix =
			new Dictionary<string, DiagnosticContextMetricsStorage>();
		
		private readonly DynamicStepsPrometheusAdapter dynamicStepsAdapter = new DynamicStepsPrometheusAdapter();
		private readonly CountersPrometheusAdapter countersAdapter = new CountersPrometheusAdapter();
		private readonly ReportedValuesPrometheusAdapter reportedValuesAdapter = new ReportedValuesPrometheusAdapter();
		
		private readonly object syncRoot = new object();
		
		public void CollectItemData(DiagnosticContextMetricsItem metricsItem)
		{
			try
			{
				lock (syncRoot)
				{
					var storage = GetOrCreateMetricStorageForMetric(metricsItem);
					storage.CollectItemData(metricsItem);

					dynamicStepsAdapter.Update(metricsItem, storage);
					countersAdapter.Update(metricsItem, storage);
					reportedValuesAdapter.Update(metricsItem, storage);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private DiagnosticContextMetricsStorage GetOrCreateMetricStorageForMetric(DiagnosticContextMetricsItem metricsItem)
		{
			if (!storagesByMetricPrefix.TryGetValue(metricsItem.MetricPrefix, out var storage))
			{
				storage = new DiagnosticContextMetricsStorage();
				storagesByMetricPrefix[metricsItem.MetricPrefix] = storage;
			}

			return storage;
		}
	}
}