using System.Text.RegularExpressions;

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class PrometheusMetricNameBuilder
	{
		private readonly string prefix;

		private readonly string? postfix;

		private static readonly Regex InvalidCharactersRegex =
			new("[^a-zA-Z0-9_:]*", RegexOptions.Compiled);

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
			InvalidCharactersRegex.Replace(metricName, "");
	}
}