// Copyright 2021 Mindbox Ltd
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using Mindbox.DiagnosticContext.MetricItem;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class PrometheusDiagnosticContextMetricsCollection : IDiagnosticContextMetricsCollection
	{	
		private readonly Dictionary<string, DiagnosticContextMetricsStorage> storagesByMetricPrefix = new();
		
		private readonly IDictionary<string, string> tags = new Dictionary<string, string>();

		private readonly DynamicStepsPrometheusAdapter dynamicStepsAdapter;
		private readonly CountersPrometheusAdapter countersAdapter;
		private readonly ReportedValuesPrometheusAdapter reportedValuesAdapter;
		
		private readonly object syncRoot = new object();

		public PrometheusDiagnosticContextMetricsCollection(
			IMetricFactory metricFactory,
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