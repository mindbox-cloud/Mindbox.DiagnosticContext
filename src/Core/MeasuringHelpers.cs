#nullable disable

using System;
using System.Collections.Generic;

namespace Mindbox.DiagnosticContext
{
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
}
