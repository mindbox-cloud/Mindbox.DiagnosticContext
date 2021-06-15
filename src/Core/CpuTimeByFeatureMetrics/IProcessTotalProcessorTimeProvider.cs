#nullable disable

using System;

namespace Mindbox.DiagnosticContext.CpuTimeByFeatureMetrics
{
	internal interface IProcessTotalProcessorTimeProvider
	{
		TimeSpan GetCurrentProcessorTime();
	}
}
