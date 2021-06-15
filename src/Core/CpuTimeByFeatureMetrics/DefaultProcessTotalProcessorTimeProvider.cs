#nullable disable

using System;
using System.Runtime.InteropServices;

namespace Mindbox.DiagnosticContext.CpuTimeByFeatureMetrics
{
	internal sealed class DefaultProcessTotalProcessorTimeProvider : IProcessTotalProcessorTimeProvider
	{
		private static readonly IntPtr currentProcessPseudoHandle = new IntPtr(-1);

		[DllImport("kernel32.dll")]
		public static extern bool GetProcessTimes(
			IntPtr handle,
			out long creationTime,
			out long exitTime,
			out long kernelTime,
			out long userTime);

		public TimeSpan GetCurrentProcessorTime()
		{
			GetProcessTimes(currentProcessPseudoHandle, out var _, out var _, out var kernelTime, out var userTime);

			return TimeSpan.FromTicks(kernelTime + userTime);
		}
	}
}
