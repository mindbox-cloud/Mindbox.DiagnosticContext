using System;
using System.Collections.Generic;
using Mindbox.DiagnosticContext.MetricItem;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class PrometheusDiagnosticContextMetricsCollection : IDiagnosticContextMetricsCollection
	{	
		private readonly Dictionary<string, DiagnosticContextMetricsStorage> storagesByMetricPrefix =
			new Dictionary<string, DiagnosticContextMetricsStorage>();
		
		private readonly IDictionary<string, string> tags = new Dictionary<string, string>();

		private readonly DynamicStepsPrometheusAdapter dynamicStepsAdapter;
		private readonly CountersPrometheusAdapter countersAdapter;
		private readonly ReportedValuesPrometheusAdapter reportedValuesAdapter;
		
		private readonly object syncRoot = new object();

		public PrometheusDiagnosticContextMetricsCollection(
			MetricFactory metricFactory,
			PrometheusMetricNameBuilder metricNameBuilder)
		{
			dynamicStepsAdapter = new DynamicStepsPrometheusAdapter(metricFactory, metricNameBuilder);
			countersAdapter = new CountersPrometheusAdapter(metricFactory, metricNameBuilder);
			reportedValuesAdapter = new ReportedValuesPrometheusAdapter(metricFactory, metricNameBuilder);
		}

		public void CollectItemData(DiagnosticContextMetricsItem metricsItem)
		{
			try
			{
				lock (syncRoot)
				{
					var storage = GetOrCreateMetricStorageForMetric(metricsItem);
					storage.CollectItemData(metricsItem);

					dynamicStepsAdapter.Update(metricsItem, storage, tags);
					countersAdapter.Update(metricsItem, storage, tags);
					reportedValuesAdapter.Update(metricsItem, storage, tags);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void SetTag(string tag, string value)
		{
			tags[tag] = value;
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