#nullable disable

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	internal class ThreadAllocatedBytesMetricsType : MetricsType<ThreadAllocatedBytesMeasurer>
	{
		public static ThreadAllocatedBytesMetricsType Create(ICurrentTimeAccessor currentTimeAccessor, string systemName)
		{
			return new ThreadAllocatedBytesMetricsType(currentTimeAccessor, systemName);
		}

		public override string Units => "[bytes]";

		private ThreadAllocatedBytesMetricsType(ICurrentTimeAccessor currentTimeAccessor, string systemName) : base(currentTimeAccessor, systemName, NullMetricsMeasurerCreationHandler.Instance)
		{
		}

		protected override ThreadAllocatedBytesMeasurer CreateMeasurerCore()
		{
			return new ThreadAllocatedBytesMeasurer(CurrentTimeAccessor, SystemName);
		}
	}
}
