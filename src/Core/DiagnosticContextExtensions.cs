using System;

namespace Mindbox.DiagnosticContext
{
	public static class DiagnosticContextExtensions
	{
		public static IDisposable MeasureForAdditionalMetric(
			this IDiagnosticContext diagnosticContext, 
			string metricPath,
			bool isFeatureBoundaryCodePoint = false)
		{
			return diagnosticContext
				?.MeasureForAdditionalMetric(
					DiagnosticContextFactory.BuildForMetric(metricPath, isFeatureBoundaryCodePoint)) 
				?? NullDisposable.Instance;
		}


	}
}
