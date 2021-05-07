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
			if (string.IsNullOrEmpty(postfix))
				return $"{prefix}_{metricName}".ToLower();
			else
				return $"{prefix}_{metricName}_{postfix}".ToLower();
		}
	}
}