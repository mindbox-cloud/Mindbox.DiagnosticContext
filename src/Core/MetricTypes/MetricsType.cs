#nullable disable

using System;

namespace Itc.Commons
{
	public abstract class MetricsType
	{
		internal MetricsType(string systemName)
		{
			if (systemName.IsNullOrEmpty())
				throw new ArgumentNullException(nameof(systemName));

			SystemName = systemName;
		}

		public string SystemName { get; }
		public virtual string Units => string.Empty;

		public virtual long ConvertMetricValue(long rawMetricValue) => rawMetricValue;
		internal abstract MetricsMeasurer CreateMeasurer();
	}

	internal abstract class MetricsType<TMetricMeasurer> : MetricsType
		where TMetricMeasurer : MetricsMeasurer
	{
		private readonly IMetricsMeasurerCreationHandler handler;

		protected MetricsType(string systemName, IMetricsMeasurerCreationHandler handler) : base(systemName)
		{
			this.handler = handler;
		}

		protected abstract TMetricMeasurer CreateMeasurerCore();

		internal override MetricsMeasurer CreateMeasurer()
		{
			var measurer = CreateMeasurerCore();
			handler.HandleMeasurerCreation(measurer);
			return measurer;
		}
	}
}
