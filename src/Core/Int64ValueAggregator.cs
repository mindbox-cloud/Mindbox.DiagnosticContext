using System;

namespace Mindbox.DiagnosticContext
{
	public class Int64ValueAggregator
	{
		private long? min;

		public long Count { get; private set; }
		public long Total { get; private set; }
		public long Max { get; private set; }

		public long Min => min ?? 0;

		public void Add(long value)
		{
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value), "value < 0");

			Total += value;
			if ((min == null) || (value < min))
				min = value;
			if (value > Max)
				Max = value;

			Count++;
		}

		public Int64MetricData ToMetricData(long? countOverride = null)
		{
			return new()
			{
				Total = Total,
				Count = countOverride ?? Count,
				Max = Max,
				Min = Min
			};
		}
	}

	public class Int64MetricData
	{
		public long Total { get; set; }
		public long? Count { get; set; }
		public long? Max { get; set; }
		public long? Min { get; set; }
	}
}
