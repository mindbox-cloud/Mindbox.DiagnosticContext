using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Filters;

namespace Mindbox.DiagnosticContext.AspNetCore
{
	[AttributeUsage(AttributeTargets.Method)]
	public class UseDiagnosticContextAttribute : ActionFilterAttribute
	{
		private readonly string metricName;
		private const string DiagnosticContextParameterName = "diagnosticContext";
		
		public UseDiagnosticContextAttribute(string metricName)
		{
			this.metricName = metricName;
		}
		
		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var diagnosticContextFactory = (IDiagnosticContextFactory)context.HttpContext.RequestServices
				.GetService(typeof(IDiagnosticContextFactory));
			
			using var diagnosticContext = diagnosticContextFactory.CreateDiagnosticContext(metricName);
			
			context.HttpContext.StoreDiagnosticContext(diagnosticContext);
			
			if (context.ActionArguments.ContainsKey(DiagnosticContextParameterName))
				context.ActionArguments[DiagnosticContextParameterName] = diagnosticContext;
			
			await next();
		}
	}
}