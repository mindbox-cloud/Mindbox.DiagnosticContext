using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mindbox.DiagnosticContext.Prometheus;

namespace Prometheus.Tests
{
	[TestClass]
	public class PrometheusMetricNameBuilderTests
	{
		[TestMethod]
		public void BuildFullName_Default_InsertsDiagnosticContextPrefix()
		{
			var nameBuilder = new PrometheusMetricNameBuilder();

			Assert.AreEqual(expected: "diagnosticcontext_test", nameBuilder.BuildFullMetricName("test"));
		}

		[TestMethod]
		public void BuildFullName_ConvertsNameToLowerCase()
		{
			var nameBuilder = new PrometheusMetricNameBuilder();

			Assert.AreEqual(expected: "diagnosticcontext_test", nameBuilder.BuildFullMetricName("TeSt"));
		}

		[TestMethod]
		public void BuildFullName_CustomPrefix_InsertsPrefix()
		{
			var nameBuilder = new PrometheusMetricNameBuilder(prefix: "dc");

			Assert.AreEqual(expected: "dc_test", nameBuilder.BuildFullMetricName("test"));
		}

		[TestMethod]
		public void BuildFullName_CustomPostfix_InsertsPostfixAndDefaultPrefix()
		{
			var nameBuilder = new PrometheusMetricNameBuilder(postfix: "tenant");

			Assert.AreEqual(expected: "diagnosticcontext_test_tenant", nameBuilder.BuildFullMetricName("test"));
		}
	}
}