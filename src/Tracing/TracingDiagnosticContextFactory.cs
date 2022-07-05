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

using OpenTracing;

namespace Mindbox.DiagnosticContext.Tracing;

internal class TracingDiagnosticContextFactory : IDiagnosticContextFactory
{
	private readonly IDiagnosticContextFactory _innerDiagnosticContextFactory;
	private readonly ITracer _tracer;
	private readonly IDiagnosticContextLogger _logger;

	public TracingDiagnosticContextFactory(
		IDiagnosticContextFactory innerDiagnosticContextFactory,
		ITracer tracer,
		IDiagnosticContextLogger logger)
	{
		_innerDiagnosticContextFactory = innerDiagnosticContextFactory;
		_tracer = tracer;
		_logger = logger;
	}

	public IDiagnosticContext CreateDiagnosticContext(
		string metricPath,
		bool isFeatureBoundaryCodePoint = false,
		MetricsType[]? metricsTypesOverride = null)
	{
		var innerDiagnosticContext = _innerDiagnosticContextFactory
			.CreateDiagnosticContext(metricPath, isFeatureBoundaryCodePoint, metricsTypesOverride);

		return new TracingDiagnosticContext(innerDiagnosticContext, _tracer, _logger);
	}
}