#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itc.Commons.Model
{
	public class NullDiagnosticContext : IDiagnosticContext
	{
		private readonly DiagnosticContextCollection diagnosticContextCollection = new DiagnosticContextCollection();
		private readonly SafeExceptionHandler safeExceptionHandler = new SafeExceptionHandler(ItcLogLevel.Error);

		private bool disposed;

		public string PrefixName => null;

		public IDisposable MeasureForAdditionalMetric(IDiagnosticContext diagnosticContext)
		{
			return safeExceptionHandler.HandleExceptions(
				() =>
				{
					diagnosticContextCollection.LinkDiagnosticContext(diagnosticContext);
					return diagnosticContext;
				},
				() => NullDisposable.Instance);
		}

		public IDisposable Measure(string stepName)
		{
			return safeExceptionHandler.HandleExceptions(
				() => diagnosticContextCollection.Measure(stepName),
				() => NullDisposable.Instance);
		}

		public void SetTag(string tag, string value)
		{
			// do nothing
		}

		public void Increment(string counterPath)
		{
			safeExceptionHandler.HandleExceptions(
				() => diagnosticContextCollection.Increment(counterPath));
		}

		public void Dispose()
		{
			if (!disposed)
			{
				safeExceptionHandler.HandleExceptions(
					() => diagnosticContextCollection.Dispose());
				disposed = true;
			}
		}

		public IDisposable ExtendCodeSourceLabel(string extensionCodeSourceLabel)
		{
			return new DisposableExtendCodeSourceLabel(extensionCodeSourceLabel);
		}

		public void ReportValue(string counterPath, long value)
		{
			// do nothing
		}
	}
}
