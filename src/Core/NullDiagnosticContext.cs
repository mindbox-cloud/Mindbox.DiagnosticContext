using System;

namespace Mindbox.DiagnosticContext
{
	public class NullDiagnosticContext : IDiagnosticContext
	{
		private readonly DiagnosticContextCollection diagnosticContextCollection = new();
		private readonly SafeExceptionHandler safeExceptionHandler = new();

		private bool disposed;

		public string PrefixName => MetricName;

		public NullDiagnosticContext() : this("NullDiagnosticContext")
		{
		}

		/// <summary>
		/// Для рефакторинга,чтобы можно было не терять имя метрики
		/// </summary>
		/// <param name="metricName"></param>
		public NullDiagnosticContext(string metricName)
		{
			MetricName = metricName;
		}

		public string MetricName { get; }
		
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
