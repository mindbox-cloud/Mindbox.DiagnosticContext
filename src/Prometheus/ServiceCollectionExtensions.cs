using Mindbox.DiagnosticContext;
using Mindbox.DiagnosticContext.Prometheus;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class PrometheusDiagnosticContextExtensions
	{
		public static IServiceCollection AddPrometheusDiagnosticContext(this IServiceCollection serviceCollection)
		{ 
			return serviceCollection.AddSingleton<IDiagnosticContextFactory, PrometheusDiagnosticContextFactory>();
		}
	}
}