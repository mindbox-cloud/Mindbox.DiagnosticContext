#nullable disable

using System;
using Vibrant.InfluxDB.Client;

namespace Mindbox.DiagnosticContext.Influx
{
	public class PerProjectTimeseriesRow
	{
		[InfluxTag(Metadata.ProjectTagName)]
		public string ProjectSystemName { get; set; }

		[InfluxTimestamp]
		public DateTime Timestamp { get; set; }
		
		public abstract class Metadata
		{
			public const string ProjectTagName = "ProjectSystemName";
		}
	}
}
