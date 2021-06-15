#nullable disable

using System;
using System.Collections.Generic;
using Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage;
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.Influx
{
	internal class InfluxDiagnosticContextMetricsCollection : IDiagnosticContextMetricsCollection
	{
		private const string InfluxDbInstanceName = "DiagnosticContext";

		private static readonly string machineName = Environment.MachineName;
		
		public void CollectItemData(DiagnosticContextMetricsItem metricsItem)
		{
			if (metricsItem == null)
				throw new ArgumentNullException(nameof(metricsItem));
			
			if (metricsItem.IsEmpty)
			{
				return;
			}
			
			var metricsStorage = new DiagnosticContextMetricsStorage();
			metricsStorage.CollectItemData(metricsItem);

			var metrics = new List<DiagnosticContextMetricsTimeseriesRow>(
				metricsStorage.DynamicStepsPerMetricPrefix.Count
				+ metricsStorage.CountersPerMetricsPrefix.Count
				+ metricsStorage.ReportedValuesPerMetricsPrefix.Count);
			
			metricsStorage.DynamicStepsPerMetricPrefix.ForEach(m => FillMetricData(metrics, m.Key, m.Value));

			metricsStorage.CountersPerMetricsPrefix.ForEach(m => FillCounters(metrics, m.Key, m.Value));

			metricsStorage.ReportedValuesPerMetricsPrefix.ForEach(m => FillReportedValues(metrics, m.Key, m.Value));

			var adapter = ApplicationHostController
				.Instance
				.Get<MetricsModelConfiguration>()
				.GetOrCreate(InfluxDbInstanceName);

			foreach (var metric in metrics)
			{
				metric.Timestamp = ApplicationHostController.Instance.Timestamp;
				metric.ProjectSystemName = ApplicationHostController.Instance.ApplicationConfiguration.ProjectSystemName;
				metric.MachineName = machineName;
				
				adapter.Write(ModelApplicationHostController.NamedObjects.Get<RetentionPolicies>().OneHour, metric);
			}
		}

		public void SetTag(string tag, string value)
		{
			// not supported
		}

		private static void FillCounters(
			ICollection<DiagnosticContextMetricsTimeseriesRow> rows, 
			string metricPrefix, 
			DiagnosticContextCountersStorage storage)
		{
			foreach (var counter in storage.Counters)
			{
				rows.Add(new DiagnosticContextMetricsTimeseriesRow
				{
					MeasurementName = metricPrefix,
					MetricName = $"Counters/{counter.Key}",
					Count = counter.Value
				});
			}
		}

		private static void FillReportedValues(
			ICollection<DiagnosticContextMetricsTimeseriesRow> rows,
			string metricPrefix,
			DiagnosticContextReportedValuesStorage storage)
		{
			foreach (var reportedValue in storage.ReportedValues)
			{
				var reportedValueData = reportedValue.Value.ToMetricData();
				
				rows.Add(new DiagnosticContextMetricsTimeseriesRow
				{
					MeasurementName = metricPrefix,
					MetricName = $"ReportedValues/{reportedValue.Key}",
					Count = reportedValueData.Count ?? 0,
					Min = reportedValueData.Min ?? 0,
					Max = reportedValueData.Max ?? 0,
					Total = reportedValueData.Total
				});
			}
		}

		private static void FillMetricData(
			ICollection<DiagnosticContextMetricsTimeseriesRow> rows,
			string metricPrefix,
			DiagnosticContextDynamicStepsAggregatedStorage storage)
		{
			var metricAggregatedValues = storage.MetricsAggregatedValues.GetMetricsAggregatedValues();

			foreach (var metricsAggregatedValue in metricAggregatedValues)
			{
				ReportAboutMetricsType(storage, rows, metricPrefix, metricsAggregatedValue);
			}
		}

		private static void ReportAboutMetricsType(
			DiagnosticContextDynamicStepsAggregatedStorage storage,
			ICollection<DiagnosticContextMetricsTimeseriesRow> rows,
			string metricPrefix,
			MetricsAggregatedValue metricsAggregatedValue)
		{
			var units = metricsAggregatedValue.MetricsType.Units;
			var metricsTypeSystemName = metricsAggregatedValue.MetricsType.SystemName;
			foreach (var step in metricsAggregatedValue.StepValues)
			{
				var stepTime = step.Value.ToMetricData(storage.ItemsCount);
				rows.Add(new DiagnosticContextMetricsTimeseriesRow
				{
					MeasurementName = metricPrefix,
					MetricName = $"{metricsTypeSystemName}/{step.Key}{units}",
					Count = stepTime.Count ?? 0,
					Min = stepTime.Min ?? 0,
					Max = stepTime.Max ?? 0,
					Total = stepTime.Total
				});
			}

			var totalValue = metricsAggregatedValue.TotalValue.ToMetricData(storage.ItemsCount);
			rows.Add(new DiagnosticContextMetricsTimeseriesRow
			{
				MeasurementName = metricPrefix,
				MetricName = $"Total/{metricsTypeSystemName}{units}",
				Count = totalValue.Count ?? 0,
				Min = totalValue.Min ?? 0,
				Max = totalValue.Max ?? 0,
				Total = totalValue.Total
			});
		}
	}
}
