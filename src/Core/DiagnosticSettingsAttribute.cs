#nullable disable

using System;

namespace Mindbox.DiagnosticContext
{
	[AttributeUsage(AttributeTargets.Method)]
	public class DiagnosticSettingsAttribute : Attribute
	{
		public string MetricName { get; }
		public bool UseInflux { get; }

		public DiagnosticSettingsAttribute(string metricName, bool useInflux = false)
		{
			MetricName = metricName;
			UseInflux = useInflux;
		}
	}
}
