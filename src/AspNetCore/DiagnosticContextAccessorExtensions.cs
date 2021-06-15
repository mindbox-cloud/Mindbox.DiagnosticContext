using Microsoft.AspNetCore.Http;

namespace Mindbox.DiagnosticContext.AspNetCore
{
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
	}
}