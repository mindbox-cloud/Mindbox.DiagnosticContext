using System;
using System.Collections.Generic;
using System.Linq;

using Itc.Commons.Model;

namespace Itc.Commons
{
	public sealed class DefaultMetricTypesConfiguration
	{
		private const string CpuTimeMetricsSystemName = "ProcessingCpuTime";
		private const string WallClockTimeMetricsSystemName = "ProcessingTime";
		private const string ThreadAllocatedBytesSystemName = "ThreadAllocatedBytes";

		private DefaultMetricTypesConfiguration()
		{
			// empty
		}

		public static readonly DefaultMetricTypesConfiguration Instance = new DefaultMetricTypesConfiguration();

		public MetricsTypeCollection GetStandardMetricsTypes()
		{
			return GetStandardMetricsTypes(NullMetricsMeasurerCreationHandler.Instance);
		}

		public MetricsTypeCollection GetAsyncMetricsTypes()
		{
			return new MetricsTypeCollection(new MetricsType[]
			{
				WallClockTimeMetricsType.Create(WallClockTimeMetricsSystemName)
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
				WallClockTimeMetricsType.Create(WallClockTimeMetricsSystemName),
				ThreadAllocatedBytesMetricsType.Create(ThreadAllocatedBytesSystemName)
			};
			if (ApplicationHostController.IsWindowsPlatform)
				// CPU metrics type uses WinAPI in CpuTimeMeasurer
				metricsTypes.Add(CpuTimeMetricsType.Create(CpuTimeMetricsSystemName, handler));
			return new MetricsTypeCollection(metricsTypes);
		}
	}
}
