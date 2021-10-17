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

namespace Mindbox.DiagnosticContext.Tracing
{
	internal class TracingDiagnosticContextDecorator : IDiagnosticContext
	{
		private readonly IDiagnosticContext diagnosticContext;
		private readonly ITracer tracer;

		private readonly SafeExceptionHandler exceptionHandler;
		
		public string PrefixName => diagnosticContext.PrefixName;

		public TracingDiagnosticContextDecorator(
			IDiagnosticContext diagnosticContext, 
			ITracer tracer, 
			IDiagnosticContextLogger logger)
		{
			this.diagnosticContext = diagnosticContext;
			this.tracer = tracer;
			exceptionHandler = new SafeExceptionHandler(e => logger.Log(e.Message, e));
		}

		public IDisposable Measure(string stepName)
		{
			return new MeasureScope(CreateMeasure(stepName).ToArray());
		}

		public IDisposable MeasureForAdditionalMetric(IDiagnosticContext diagnosticContext)
		{
			return diagnosticContext.MeasureForAdditionalMetric(diagnosticContext);
		}

		public void SetTag(string tag, string value)
		{
			diagnosticContext.SetTag(tag, value);
			exceptionHandler.Execute(() => tracer.ActiveSpan.SetTag(tag, value));
		}

		public void Increment(string counterPath)
		{
			diagnosticContext.Increment(counterPath);
		}

		public IDisposable ExtendCodeSourceLabel(string extensionCodeSourceLabel)
		{
			return diagnosticContext.ExtendCodeSourceLabel(extensionCodeSourceLabel);
		}

		public void ReportValue(string counterPath, long value)
		{
			diagnosticContext.ReportValue(counterPath, value);
		}

		public void Dispose()
		{
			diagnosticContext.Dispose();
		}

		private IEnumerable<IDisposable> CreateMeasure(string stepName)
		{
			yield return diagnosticContext.Measure(stepName);
			yield return exceptionHandler.Execute<IDisposable>(
				action: () => tracer.BuildSpan(stepName).StartActive(),
				fallback: () => new EmptyMeasureScope());
		}
	}
}