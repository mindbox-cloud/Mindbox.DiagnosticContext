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