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

using Mindbox.DiagnosticContext;
using Mindbox.DiagnosticContext.MetricsTypes;
using Mindbox.DiagnosticContext.Prometheus;

namespace Microsoft.Extensions.DependencyInjection;

public static class PrometheusDiagnosticContextExtensions
{
	/// <summary>
	/// Adds all the necessary dependencies to collect metrics in Prometheus.
	/// </summary>
	/// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add the service to.</param>
	/// <param name="metricPrefix">String constant which will be added at the beginning of each metric name.
	/// It is strongly recommended to use a unique prefix that includes the name of the application -
	/// this can guarantee that there is no intersection of metrics.</param>
	/// <returns></returns>
	public static IServiceCollection AddPrometheusDiagnosticContext(
		this IServiceCollection serviceCollection,
		string? prefix = null)
		=> serviceCollection.AddSingleton<IDiagnosticContextFactory, PrometheusDiagnosticContextFactory>(
			serviceProvider =>
				new PrometheusDiagnosticContextFactory(
					serviceProvider.GetRequiredService<DefaultMetricTypesConfiguration>(),
					serviceProvider.GetRequiredService<IDiagnosticContextLogger>(),
					metricPrefix: prefix));
}