using Microsoft.AspNetCore.Mvc;
using Mindbox.DiagnosticContext.AspNetCore;

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
				using (diagnosticContext.Measure("inner"))
					return Ok();
			}
		}
	}
}