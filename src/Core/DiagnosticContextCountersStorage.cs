#nullable disable

using System.Collections.Generic;

namespace Itc.Commons
{
	public class DiagnosticContextCountersStorage
	{
		private readonly Dictionary<string, int> counters = new Dictionary<string, int>();

		public IReadOnlyDictionary<string, int> Counters => counters;

		public void CollectItemData(Dictionary<string, int> itemCounters)
		{
			foreach (var itemCounter in itemCounters)
			{
				if (!counters.ContainsKey(itemCounter.Key))
					counters[itemCounter.Key] = 0;
				counters[itemCounter.Key] += itemCounter.Value;
			}
		}
	}
}
