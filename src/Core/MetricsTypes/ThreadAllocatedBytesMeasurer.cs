#nullable disable

using System;
using System.Linq.Expressions;

namespace Mindbox.DiagnosticContext.MetricsTypes
{
	internal sealed class ThreadAllocatedBytesMeasurer : MetricsMeasurer
	{
		private static readonly Func<long> getAllocatedBytesForCurrentThread;

		private long bytesAllocatedByThreadAtStart;

		static ThreadAllocatedBytesMeasurer()
		{
			var getAllocatedBytesForCurrentThreadMethod = typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread");
			if (getAllocatedBytesForCurrentThreadMethod == null)
			{
				getAllocatedBytesForCurrentThread = () => 0;
			}
			else
			{
				getAllocatedBytesForCurrentThread =
					Expression.Lambda<Func<long>>(Expression.Call(getAllocatedBytesForCurrentThreadMethod))
						.Compile();
			}
		}

		public ThreadAllocatedBytesMeasurer(ICurrentTimeAccessor currentTimeAccessor, string metricsTypeSystemName) : base(currentTimeAccessor, metricsTypeSystemName)
		{
		}

		protected override long? GetValueCore()
		{
			return bytesAllocatedByThreadAtStart;
		}

		protected override void StartCore()
		{
			bytesAllocatedByThreadAtStart += getAllocatedBytesForCurrentThread();
		}

		protected override void StopCore()
		{
			bytesAllocatedByThreadAtStart = getAllocatedBytesForCurrentThread() - bytesAllocatedByThreadAtStart;
		}
	}
}
