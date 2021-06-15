using System.Collections.Concurrent;
using System.Threading;

namespace Mindbox.DiagnosticContext
{
	public static class CallContext
	{
		private static readonly ConcurrentDictionary<string, AsyncLocal<object?>> state = new();

		public static void LogicalSetData(string name, object? data) =>
			state.GetOrAdd(name, _ => new AsyncLocal<object?>()).Value = data;

		public static object? LogicalGetData(string name) =>
			state.TryGetValue(name, out AsyncLocal<object?> data) ? data.Value : null;

		public static void FreeNamedDataSlot(string name)
		{
			state.GetOrAdd(name, _ => new AsyncLocal<object?>()).Value = null;
		}
	}
}
