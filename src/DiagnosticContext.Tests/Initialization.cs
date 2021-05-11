using Itc.Commons.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiagnosticContext.Tests
{
	[TestClass]
	public class Initialization
	{
		[AssemblyInitialize]
		public static void InitializeAssembly(TestContext context)
		{
			TestApplicationController.InitializeTestAssembly();
		}
	}
}