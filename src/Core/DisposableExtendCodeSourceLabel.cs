#nullable disable

using System;
using System.Collections.Generic;
using System.Text;

namespace Itc.Commons.Model
{
	public class DisposableExtendCodeSourceLabel : IDisposable
	{
		private readonly string originalCodeSourceLabel;

		private bool disposed;

		public DisposableExtendCodeSourceLabel(string extensionCodeSourceLabel)
		{
			originalCodeSourceLabel = CodeFlowMonitoringService.TryGetCodeSourceLabel();

			var newCodeSourceLabel = string.IsNullOrWhiteSpace(originalCodeSourceLabel)
				? extensionCodeSourceLabel
				: $"{originalCodeSourceLabel}/{extensionCodeSourceLabel}";

			CodeFlowMonitoringService.SetCodeSourceLabel(newCodeSourceLabel);
		}

		public void Dispose()
		{
			if (!disposed)
			{
				CodeFlowMonitoringService.SetCodeSourceLabel(originalCodeSourceLabel);

				disposed = true;
			}
		}
	}
}
