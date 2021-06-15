using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;

#nullable disable

namespace Mindbox.DiagnosticContext.Influx
{
	internal class DiagnosticContextMetricsTimeseriesRow : PerProjectTimeseriesRow, IHaveMeasurementName
	{
		[InfluxField("Min")]
		public double Min { get; set; }
		
		[InfluxField("Max")]
		public double Max { get; set; }
		
		[InfluxField("Total")]
		public double Total { get; set; }
		
		[InfluxField("Count")]
		public double Count { get; set; }
		
		[InfluxTag("MetricName")]
		public string MetricName { get; set; }
		
		[InfluxTag("MachineName")]
		public string MachineName { get; set; }

		public string MeasurementName { get; set; }
	}
}
