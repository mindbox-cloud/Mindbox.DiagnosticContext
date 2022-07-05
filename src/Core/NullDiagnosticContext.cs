// Copyright 2021 Mindbox Ltd
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Mindbox.DiagnosticContext;

public class NullDiagnosticContext : IDiagnosticContext
{
	private readonly DiagnosticContextCollection _diagnosticContextCollection = new();
	private readonly SafeExceptionHandler _safeExceptionHandler = new();

	private bool _disposed;

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
		return _safeExceptionHandler.HandleExceptions(
			() =>
			{
				_diagnosticContextCollection.LinkDiagnosticContext(diagnosticContext);
				return diagnosticContext;
			},
			() => NullDisposable.Instance);
	}

	public IDisposable Measure(string stepName)
	{
		return _safeExceptionHandler.HandleExceptions(
			() => _diagnosticContextCollection.Measure(stepName),
			() => NullDisposable.Instance);
	}

	public void SetTag(string tag, string value)
	{
		// do nothing
	}

	public void Increment(string counterPath)
	{
		_safeExceptionHandler.HandleExceptions(
			() => _diagnosticContextCollection.Increment(counterPath));
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_safeExceptionHandler.HandleExceptions(
				() => _diagnosticContextCollection.Dispose());
			_disposed = true;
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