using System;

namespace Mindbox.DiagnosticContext
{
	public static class CodeFlowMonitoringService
	{
		private const string CodeSourceLabelKey = "CodeSourceLabelKey";

		public static string? TryGetCodeSourceLabel()
		{
			return (string?)CallContext.LogicalGetData(CodeSourceLabelKey);
		}

		public static IDisposable SetCodeSourceLabel(string codeSourceLabel)
		{
			CallContext.LogicalSetData(CodeSourceLabelKey, codeSourceLabel);

			return new Cleaner(CodeSourceLabelKey);
		}

		public static void Clear()
		{
			CallContext.FreeNamedDataSlot(CodeSourceLabelKey);
		}

		public static IDisposable TrySetCodeSourceLabel(string codeSourceLabel)
		{
			var currentCodeSourceLabel = TryGetCodeSourceLabel();
			if (currentCodeSourceLabel != null)
				return NullDisposable.Instance;

			return SetCodeSourceLabel(codeSourceLabel);
		}

		private class Cleaner : IDisposable
		{
			private readonly string name;
			private bool isDisposed;

			public Cleaner(string name)
			{
				this.name = name;
			}

			public void Dispose()
			{
				if (isDisposed)
				{
					return;
				}

				CallContext.FreeNamedDataSlot(name);
				isDisposed = true;
			}
		}
	}
}
