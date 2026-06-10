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
using Mindbox.DiagnosticContext;
using Mindbox.DiagnosticContext.MetricsTypes;
using Mindbox.DiagnosticContext.Prometheus;

namespace Microsoft.Extensions.DependencyInjection;

public static class PrometheusDiagnosticContextExtensions
{
	/// <summary>
	/// Adds all the necessary dependencies to collect metrics in Prometheus.
	/// Metric series live indefinitely (no automatic eviction).
	/// </summary>
	[Obsolete("Use AddPrometheusDiagnosticContextWithManagedLifetime to enable automatic eviction of inactive series.")]
	public static IServiceCollection AddPrometheusDiagnosticContext(
		this IServiceCollection serviceCollection,
		string? prefix = null)
#pragma warning disable CS0618
		=> serviceCollection.AddSingleton<IDiagnosticContextFactory, PrometheusDiagnosticContextFactory>(
			serviceProvider =>
				new PrometheusDiagnosticContextFactory(
					serviceProvider.GetRequiredService<DefaultMetricTypesConfiguration>(),
					serviceProvider.GetRequiredService<IDiagnosticContextLogger>(),
					metricPrefix: prefix));
#pragma warning restore CS0618

	/// <summary>
	/// Adds all the necessary dependencies to collect metrics in Prometheus
	/// with automatic eviction of inactive time series.
	/// </summary>
	/// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add the service to.</param>
	/// <param name="metricLifetime">Time after which an unused metric series is automatically removed.
	/// Defaults to <see cref="PrometheusDiagnosticContextFactory.DefaultMetricLifetime"/> (5 minutes).</param>
	/// <param name="prefix">Prefix added to each metric name.</param>
	public static IServiceCollection AddPrometheusDiagnosticContextWithManagedLifetime(
		this IServiceCollection serviceCollection,
		TimeSpan? metricLifetime = null,
		string? prefix = null)
		=> serviceCollection.AddSingleton<IDiagnosticContextFactory, PrometheusDiagnosticContextFactory>(
			serviceProvider =>
				new PrometheusDiagnosticContextFactory(
					serviceProvider.GetRequiredService<DefaultMetricTypesConfiguration>(),
					serviceProvider.GetRequiredService<IDiagnosticContextLogger>(),
					metricLifetime ?? PrometheusDiagnosticContextFactory.DefaultMetricLifetime,
					metricPrefix: prefix));
}
