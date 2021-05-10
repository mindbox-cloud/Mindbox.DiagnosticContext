#nullable disable

using System;

namespace Itc.Commons
{
	internal class ThreadAllocatedBytesMetricsType : MetricsType<ThreadAllocatedBytesMeasurer>
	{
		public static ThreadAllocatedBytesMetricsType Create(string systemName)
		{
			return new ThreadAllocatedBytesMetricsType(systemName);
		}

		public override string Units => "[bytes]";

		private ThreadAllocatedBytesMetricsType(string systemName) : base(systemName, NullMetricsMeasurerCreationHandler.Instance)
		{
		}

		protected override ThreadAllocatedBytesMeasurer CreateMeasurerCore()
		{
			return new ThreadAllocatedBytesMeasurer(SystemName);
		}
	}
}
