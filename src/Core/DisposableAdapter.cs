#nullable disable

using System;

namespace Mindbox.DiagnosticContext
{
	internal class DisposableAdapter<TAdaptee> : IDisposable
	{
		private readonly TAdaptee adaptee;
		private readonly Action<TAdaptee> onDispose;

		public DisposableAdapter(TAdaptee adaptee, Action<TAdaptee> onCreation, Action<TAdaptee> onDispose)
		{
			if (onCreation == null)
				throw new ArgumentNullException(nameof(onCreation));
			if (onDispose == null)
				throw new ArgumentNullException(nameof(onDispose));

			this.adaptee = adaptee;
			this.onDispose = onDispose;
			onCreation(adaptee);
		}

		public void Dispose()
		{
			onDispose(adaptee);
		}
	}
}
