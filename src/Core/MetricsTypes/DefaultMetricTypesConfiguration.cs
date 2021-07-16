using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	public sealed class DefaultMetricTypesConfiguration
	{
		private readonly ICurrentTimeAccessor currentTimeAccessor;
		private const string CpuTimeMetricsSystemName = "ProcessingCpuTime";
		private const string WallClockTimeMetricsSystemName = "ProcessingTime";
		private const string ThreadAllocatedBytesSystemName = "ThreadAllocatedBytes";

		public DefaultMetricTypesConfiguration(ICurrentTimeAccessor? currentTimeAccessor = null)
		{
			this.currentTimeAccessor = currentTimeAccessor ?? new DefaultCurrentTimeAccessor();
		}

		public MetricsTypeCollection GetStandardMetricsTypes()
		{
			return GetStandardMetricsTypes(NullMetricsMeasurerCreationHandler.Instance);
		}

		public MetricsTypeCollection GetAsyncMetricsTypes()
		{
			return new(new MetricsType[]
			{
				WallClockTimeMetricsType.Create(currentTimeAccessor, WallClockTimeMetricsSystemName)
			});
		}

		public MetricsTypeCollection GetMetricTypesWithThreadAllocatedBytes()
		{
			return new(new MetricsType[]
			{
				WallClockTimeMetricsType.Create(currentTimeAccessor, WallClockTimeMetricsSystemName),
				ThreadAllocatedBytesMetricsType.Create(currentTimeAccessor, ThreadAllocatedBytesSystemName)
			});
		}

		public MetricsTypeCollection GetMetricsTypesWithCpuTimeTracking(string featureSystemName)
		{
			return GetStandardMetricsTypes(NullMetricsMeasurerCreationHandler.Instance);
		}

		private MetricsTypeCollection GetStandardMetricsTypes(IMetricsMeasurerCreationHandler handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			var metricsTypes = new List<MetricsType>
			{
				WallClockTimeMetricsType.Create(currentTimeAccessor, WallClockTimeMetricsSystemName),
				ThreadAllocatedBytesMetricsType.Create(currentTimeAccessor, ThreadAllocatedBytesSystemName)
			};
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// CPU metrics type uses WinAPI in CpuTimeMeasurer
				metricsTypes.Add(CpuTimeMetricsType.Create(currentTimeAccessor, CpuTimeMetricsSystemName, handler));
			}

			return new MetricsTypeCollection(metricsTypes);
		}
	}
}
