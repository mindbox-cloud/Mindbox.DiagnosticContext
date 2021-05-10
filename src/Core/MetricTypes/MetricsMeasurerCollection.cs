#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;

using Itc.Commons.Model;

namespace Itc.Commons
{
	internal class MetricsMeasurerCollection
	{
		private static readonly SafeExceptionHandler handler = new SafeExceptionHandler(ItcLogLevel.Error);

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
