using Itc.Commons.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Prometheus.Tests
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