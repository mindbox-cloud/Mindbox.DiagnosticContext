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

using Mindbox.DiagnosticContext.MetricsTypes;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus;

public class PrometheusDiagnosticContextFactory : IDiagnosticContextFactory
{
	private readonly DefaultMetricTypesConfiguration _defaultMetricTypesConfiguration;
	private readonly IDiagnosticContextLogger _diagnosticContextLogger;
	private readonly IMetricFactory _metricFactory;
	private readonly PrometheusMetricNameBuilder _metricNameBuilder;

	public PrometheusDiagnosticContextFactory(
		DefaultMetricTypesConfiguration defaultMetricTypesConfiguration,
		IDiagnosticContextLogger diagnosticContextLogger,
		IMetricFactory? metricFactory = null,
		string? metricPostfix = null,
		string? metricPrefix = null)
	{
		_defaultMetricTypesConfiguration = defaultMetricTypesConfiguration;
		_diagnosticContextLogger = diagnosticContextLogger;
		_metricFactory = metricFactory ?? Metrics.WithCustomRegistry(Metrics.DefaultRegistry);
		_metricNameBuilder = new PrometheusMetricNameBuilder(prefix: metricPrefix, postfix: metricPostfix);
	}

	public IDiagnosticContext CreateDiagnosticContext(
		string metricPath,
		bool isFeatureBoundaryCodePoint = false,
		MetricsType[]? metricsTypesOverride = null)
	{
		var collection = new PrometheusDiagnosticContextMetricsCollection(_metricFactory, _metricNameBuilder);

		return DiagnosticContextFactory.BuildCustom(
			() =>
			{
				var metricTypes = metricsTypesOverride != null ?
						new MetricsTypeCollection(metricsTypesOverride)
						: _defaultMetricTypesConfiguration.GetMetricsTypesWithCpuTimeTracking(metricPath);

				return new DiagnosticContext(
					_diagnosticContextLogger,
					metricPath,
					collection,
					metricTypes,
					isFeatureBoundaryCodePoint);
			}, _diagnosticContextLogger);
	}
}