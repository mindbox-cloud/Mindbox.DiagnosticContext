#nullable disable

using System;
using System.Collections.Generic;

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	internal class MetricsMeasurerCollection
	{
		private static readonly SafeExceptionHandler handler = new SafeExceptionHandler();

		public MetricsMeasurerCollection(ICollection<MetricsMeasurer> measurers)
		{
			if (measurers == null)
				throw new ArgumentNullException(nameof(measurers));

			this.Measurers = measurers;
		}

		public void Start()
		{
			foreach (var metricsMeasurer in Measurers)
			{
				handler.HandleExceptions(metricsMeasurer.Start);
			}
		}

		public void Stop()
		{
			foreach (var metricsMeasurer in Measurers)
			{
				handler.HandleExceptions(() => metricsMeasurer.Stop());
			}
		}

		public ICollection<MetricsMeasurer> Measurers { get; }
	}
}
