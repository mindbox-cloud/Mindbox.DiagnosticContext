using System;

namespace Mindbox.DiagnosticContext
{
	public interface IDiagnosticContext : IDisposable
	{
		string PrefixName { get; }

		IDisposable MeasureForAdditionalMetric(IDiagnosticContext diagnosticContext);

		IDisposable Measure(string stepName);

		void SetTag(string tag, string value);

		void Increment(string counterPath);

		IDisposable ExtendCodeSourceLabel(string extensionCodeSourceLabel);

		void ReportValue(string counterPath, long value);
	}
}
