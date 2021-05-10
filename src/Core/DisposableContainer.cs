#nullable disable

using System;

namespace Itc.Commons.Model
{
	internal class DisposableContainer : IDisposable
	{
		private readonly IDisposable[] items;

		private bool disposed;

		public DisposableContainer(params IDisposable[] items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			this.items = items;
		}

		public void Dispose()
		{
			if (!disposed)
			{
				foreach (var item in items)
					item?.Dispose();

				disposed = true;
			}
		}
	}
}
