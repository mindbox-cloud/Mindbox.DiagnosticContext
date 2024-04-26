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

using Microsoft.AspNetCore.Http;

namespace Mindbox.DiagnosticContext.AspNetCore;

public static class DiagnosticContextHttpContextExtensions
{
	private const string DiagnosticContextItemKey = "DiagnosticContext";

	public static IDiagnosticContext GetDiagnosticContext(this HttpContext httpContext)
	{
		return httpContext.Items[DiagnosticContextItemKey] as IDiagnosticContext ?? new NullDiagnosticContext();
	}

	internal static void StoreDiagnosticContext(this HttpContext httpContext, IDiagnosticContext diagnosticContext)
	{
		httpContext.Items.Add(DiagnosticContextItemKey, diagnosticContext);
	}

	internal static void OverrideDiagnosticContext(this HttpContext httpContext, IDiagnosticContext newDiagnosticContext)
	{
		httpContext.Items.Remove(DiagnosticContextItemKey);
		httpContext.Items.Add(DiagnosticContextItemKey, newDiagnosticContext);
	}
}