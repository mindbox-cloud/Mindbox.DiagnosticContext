#nullable disable

using System.Collections.Generic;

namespace Mindbox.DiagnosticContext
{
	public class DiagnosticContextReportedValuesStorage
	{
		private readonly Dictionary<string, Int64ValueAggregator> reportedValues = new Dictionary<string, Int64ValueAggregator>();

		public IReadOnlyDictionary<string, Int64ValueAggregator> ReportedValues => reportedValues;
		
		public void CollectItemData(Dictionary<string, long> itemReportedValues)
		{
			foreach (var itemCounter in itemReportedValues)
			{
				if (!reportedValues.ContainsKey(itemCounter.Key))
					reportedValues.Add(itemCounter.Key, new Int64ValueAggregator());
				
				reportedValues[itemCounter.Key].Add(itemCounter.Value);
			}
		}
	}
}
