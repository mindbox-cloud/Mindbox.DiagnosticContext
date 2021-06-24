﻿using System.Collections.Generic;
using System.Linq;

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	public class MetricsTypeCollection
	{
		public ICollection<MetricsType> MetricsTypes { get; }

		public MetricsTypeCollection(ICollection<MetricsType> metricsTypes)
		{
			this.MetricsTypes = metricsTypes;
		}

		internal MetricsMeasurerCollection CreateMeasurers()
		{
			var measurers = MetricsTypes.Select(mt => mt.CreateMeasurer()).ToList();

			return new MetricsMeasurerCollection(measurers);
		}
	}
}