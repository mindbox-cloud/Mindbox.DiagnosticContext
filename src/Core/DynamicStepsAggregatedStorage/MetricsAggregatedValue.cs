#nullable disable

using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage
{
	public class MetricsAggregatedValue
	{
		public MetricsAggregatedValue(MetricsType metricsType)
		{
			MetricsType = metricsType;
		}

		public MetricsType MetricsType { get; }

		public Dictionary<string, Int64ValueAggregator> StepValues { get; } = new Dictionary<string, Int64ValueAggregator>();
		public Int64ValueAggregator TotalValue { get; } = new Int64ValueAggregator();

		public void Add(DiagnosticContextMetricsNormalizedValue metricsNormalizedValue)
		{
			foreach (var step in metricsNormalizedValue.NormalizedValues)
			{
				if (!StepValues.TryGetValue(step.Key, out var aggregator))
				{
					aggregator = new Int64ValueAggregator();
					StepValues.Add(step.Key, aggregator);
				}
				aggregator.Add(step.Value);
			}
			TotalValue.Add(metricsNormalizedValue.NormalizedValues.Sum(step => step.Value));
		}
	}
}
