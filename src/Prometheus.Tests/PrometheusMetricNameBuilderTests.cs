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

		[TestMethod]
		[DataRow(" ")]
		[DataRow("-")]
		[DataRow(".")]
		public void BuildFullName_MetricNameHasInvalidCharacters_ReturnsValidMetricName(string badChar)
		{
			const string metricName = "metric_name";
			const string postfix = "postfix";

			var metricNameWithInvalidCharacters = badChar + metricName;
			var postfixWithInvalidCharacters = badChar + postfix + badChar;

			var actualMetricName = new PrometheusMetricNameBuilder(postfix: postfixWithInvalidCharacters)
				.BuildFullMetricName(metricNameWithInvalidCharacters);
			
			Assert.AreEqual($"diagnosticcontext_{metricName}_{postfix}", actualMetricName);
		}
	}
}