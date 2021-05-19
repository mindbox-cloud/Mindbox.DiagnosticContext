using System.Text.RegularExpressions;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class PrometheusMetricNameBuilder
	{
		private readonly string prefix;

		private readonly string? postfix;

		public PrometheusMetricNameBuilder(string prefix = "diagnosticcontext", string? postfix = null)
		{
			this.prefix = prefix;
			this.postfix = postfix;
		}

		public string BuildFullMetricName(string metricName)
		{
			var fullMetricName = string.IsNullOrEmpty(postfix)
				? $"{prefix}_{metricName}".ToLower()
				: $"{prefix}_{metricName}_{postfix}".ToLower();

			return RemoveInvalidCharactersFromMetricName(fullMetricName);
		}

		private static string RemoveInvalidCharactersFromMetricName(string metricName) =>
			Regex.Replace(metricName, "[^a-zA-Z_:][^a-zA-Z0-9_:]*", "");
	}
}