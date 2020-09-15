using System;
using Microsoft.AspNetCore.Mvc;
using Mindbox.DiagnosticContext.AspNetCore;
using Mindbox.DiagnosticContext.Prometheus;

namespace AspNetCoreTestProject.Controllers
{
	public class TestController : Controller
	{
		[HttpGet("method-injection")]
		[UseDiagnosticContext("test")]
		public ActionResult MethodInjection()
		{
			var diagnosticContext = HttpContext.GetDiagnosticContext();

			using (diagnosticContext.Measure("outer"))
			{
				diagnosticContext.ReportValue("reported", DateTime.Now.Minute);
				using (diagnosticContext.Measure("inner"))
					return Ok();
			}
		}
	}
}