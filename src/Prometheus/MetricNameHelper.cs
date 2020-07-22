namespace Mindbox.DiagnosticContext.Prometheus
{
	internal static class MetricNameHelper
	{
		private const string PrometheusMetricPrefix = "diagnosticcontext";

		public static string BuildFullMetricName(string metricName)
		{
			return $"{PrometheusMetricPrefix}_{metricName}".ToLower();
		}
	}
}