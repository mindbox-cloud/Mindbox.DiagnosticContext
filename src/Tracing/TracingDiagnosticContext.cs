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
using System.Collections.Generic;
using System.Linq;
using OpenTracing;

namespace Mindbox.DiagnosticContext.Tracing;

internal class TracingDiagnosticContext : IDiagnosticContext
{
	private readonly IDiagnosticContext _innerDiagnosticContext;
	private readonly ITracer _tracer;

	private readonly SafeExceptionHandler _exceptionHandler;

	public string PrefixName => _innerDiagnosticContext.PrefixName;

	public TracingDiagnosticContext(
		IDiagnosticContext innerDiagnosticContext,
		ITracer tracer,
		IDiagnosticContextLogger logger)
	{
		_innerDiagnosticContext = innerDiagnosticContext;
		_tracer = tracer;
		_exceptionHandler = new SafeExceptionHandler(e => logger.Log(e.Message, e));
	}

	public IDisposable Measure(string stepName)
	{
		return new MeasureScope(CreateMeasures(stepName).ToArray());
	}

	public IDisposable MeasureForAdditionalMetric(IDiagnosticContext diagnosticContext)
	{
		return _innerDiagnosticContext.MeasureForAdditionalMetric(diagnosticContext);
	}

	public void SetTag(string tag, string value)
	{
		_innerDiagnosticContext.SetTag(tag, value);
		_exceptionHandler.Execute(() => _tracer.ActiveSpan.SetTag(tag, value));
	}

	public void Increment(string counterPath)
	{
		_innerDiagnosticContext.Increment(counterPath);
	}

	public IDisposable ExtendCodeSourceLabel(string extensionCodeSourceLabel)
	{
		return _innerDiagnosticContext.ExtendCodeSourceLabel(extensionCodeSourceLabel);
	}

	public void ReportValue(string counterPath, long value)
	{
		_innerDiagnosticContext.ReportValue(counterPath, value);
	}

	public void Dispose()
	{
		_innerDiagnosticContext.Dispose();
	}

	private IEnumerable<IDisposable> CreateMeasures(string stepName)
	{
		yield return _innerDiagnosticContext.Measure(stepName);
		yield return _exceptionHandler.Execute<IDisposable>(
			action: () => _tracer.BuildSpan(stepName).StartActive(),
			fallback: () => new EmptyMeasureScope());
	}
}