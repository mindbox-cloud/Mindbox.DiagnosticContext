// Copyright 2021 Mindbox Ltd
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#nullable disable

using System.Collections.Generic;

namespace Mindbox.DiagnosticContext;

public class DiagnosticContextReportedValuesStorage
{
	private readonly Dictionary<string, Int64ValueAggregator> _reportedValues = new();

	public IReadOnlyDictionary<string, Int64ValueAggregator> ReportedValues => _reportedValues;

	public void CollectItemData(Dictionary<string, long> itemReportedValues)
	{
		foreach (var itemCounter in itemReportedValues)
		{
			if (!_reportedValues.ContainsKey(itemCounter.Key))
				_reportedValues.Add(itemCounter.Key, new Int64ValueAggregator());

			_reportedValues[itemCounter.Key].Add(itemCounter.Value);
		}
	}
}