using System;

namespace Mindbox.DiagnosticContext
{
	public abstract class MetricsType
	{
		protected ICurrentTimeAccessor CurrentTimeAccessor { get; }

		internal MetricsType(ICurrentTimeAccessor currentTimeAccessor, string systemName)
		{
			if (string.IsNullOrEmpty(systemName))
				throw new ArgumentNullException(nameof(systemName));
			this.CurrentTimeAccessor = currentTimeAccessor;

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

		protected MetricsType(ICurrentTimeAccessor currentTimeAccessor, string systemName, IMetricsMeasurerCreationHandler handler) 
			: base(currentTimeAccessor, systemName)
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
