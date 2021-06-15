using System;

namespace Mindbox.DiagnosticContext
{
	internal sealed class NullDisposable : IDisposable
	{
		public static IDisposable Instance { get; } = new NullDisposable();


		private NullDisposable()
		{
		}


		public void Dispose()
		{
		}
	}
}
