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
using Mindbox.DiagnosticContext.MetricsTypes;
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus
{
	public class PrometheusDiagnosticContextFactory : IDiagnosticContextFactory
	{
		private readonly DefaultMetricTypesConfiguration defaultMetricTypesConfiguration;
		private readonly IDiagnosticContextLogger diagnosticContextLogger;
		private readonly IMetricFactory metricFactory;
		private readonly PrometheusMetricNameBuilder metricNameBuilder;

		public PrometheusDiagnosticContextFactory(
			DefaultMetricTypesConfiguration defaultMetricTypesConfiguration, 
			IDiagnosticContextLogger diagnosticContextLogger, 
			IMetricFactory? metricFactory = null, 
			string? metricPostfix = null)
		{
			this.defaultMetricTypesConfiguration = defaultMetricTypesConfiguration;
			this.diagnosticContextLogger = diagnosticContextLogger;
			this.metricFactory = metricFactory ?? Metrics.WithCustomRegistry(Metrics.DefaultRegistry);
			this.metricNameBuilder = new PrometheusMetricNameBuilder(postfix: metricPostfix);
		}

		public IDiagnosticContext CreateDiagnosticContext(
			string metricPath,
			bool isFeatureBoundaryCodePoint = false,
			MetricsType[]? metricsTypesOverride = null)
		{
			var collection = new PrometheusDiagnosticContextMetricsCollection(metricFactory, metricNameBuilder);
				
			return DiagnosticContextFactory.BuildCustom(
				() =>
				{
					var metricTypes = metricsTypesOverride != null ?
							new MetricsTypeCollection(metricsTypesOverride) 
							: defaultMetricTypesConfiguration.GetMetricsTypesWithCpuTimeTracking(metricPath);

					return new DiagnosticContext(
						diagnosticContextLogger,
						metricPath,
						collection,
						metricTypes,
						isFeatureBoundaryCodePoint);
				}, diagnosticContextLogger);
		}
	}
}