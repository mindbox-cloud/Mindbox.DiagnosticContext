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

using System;
using System.Collections.Generic;

namespace Mindbox.DiagnosticContext;

public static class MeasuringHelpers
{
	public static void ExecuteForeachWithMeasurements<TItem>(
		this IEnumerable<TItem> enumerable,
		IDiagnosticContext diagnosticContext,
		string measurementName,
		Action<TItem> action)
	{
		using (diagnosticContext.Measure(measurementName))
		{
			IEnumerator<TItem> enumerator;
			using (diagnosticContext.Measure("GetEnumerator"))
				enumerator = enumerable.GetEnumerator();

			try
			{
				while (enumerator.MoveNext())
				{
					using (diagnosticContext.Measure("Action"))
						action(enumerator.Current);
				}
			}
			finally
			{
				enumerator.Dispose();
			}
		}
	}
}