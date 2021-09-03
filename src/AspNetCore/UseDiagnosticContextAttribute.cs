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