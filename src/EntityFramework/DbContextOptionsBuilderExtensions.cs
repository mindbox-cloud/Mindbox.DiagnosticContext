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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mindbox.DiagnosticContext.EntityFramework;

public static class EntityFrameworkDiagnosticContextExtensions
{
	public static DbContextOptionsBuilder AddEfCommandsMetrics(this DbContextOptionsBuilder serviceCollection)
	{
		return serviceCollection
			.AddInterceptors(new EfCommandsScorerInterceptor([new EfCommandsMetricsCounter()]));
	}

	public static DbContextOptionsBuilder AddEfCommandsMetrics(
		this DbContextOptionsBuilder optionsBuilder,
		IServiceCollection services)
	{
		services.AddSingleton<IEfCommandMetricsCounter, EfCommandsMetricsCounter>();
		services.AddSingleton<EfCommandsScorerInterceptor>();

		var serviceProvider = services.BuildServiceProvider();
		var interceptor = serviceProvider.GetRequiredService<EfCommandsScorerInterceptor>();

		return optionsBuilder.AddInterceptors(interceptor);
	}
}
