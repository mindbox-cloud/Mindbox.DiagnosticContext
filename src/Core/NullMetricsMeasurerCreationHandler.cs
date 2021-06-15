namespace Mindbox.DiagnosticContext
{
	internal class NullMetricsMeasurerCreationHandler : IMetricsMeasurerCreationHandler
	{
		public static NullMetricsMeasurerCreationHandler Instance { get; } = new NullMetricsMeasurerCreationHandler();

		private NullMetricsMeasurerCreationHandler()
		{
			
		}

		public void HandleMeasurerCreation(MetricsMeasurer measurer)
		{
			// empty
		}
	}
}
