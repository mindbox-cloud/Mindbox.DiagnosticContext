#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mindbox.DiagnosticContext
{
	internal class DiagnosticContextCollection : IDisposable
	{
		private readonly List<IDiagnosticContext> linkedContexts = new List<IDiagnosticContext>();

		internal void LinkDiagnosticContext(IDiagnosticContext context)
		{
			linkedContexts.Add(context);
		}

		public IDisposable Measure(string stepName)
		{
			return new DisposableContainer(
				linkedContexts
					.Select(context => context.Measure(stepName))
					.ToArray());
		}

		public void Increment(string counterPath)
		{
			foreach (var context in linkedContexts)
				context.Increment(counterPath);
		}

		public void Dispose()
		{
			foreach (var linkedContext in linkedContexts)
				linkedContext.Dispose();
		}
	}
}
