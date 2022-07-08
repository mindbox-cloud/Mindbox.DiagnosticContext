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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mindbox.DiagnosticContext.Prometheus;

namespace Prometheus.Tests;

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

		var actualMetricFullName = new PrometheusMetricNameBuilder(postfix: postfixWithInvalidCharacters)
			.BuildFullMetricName(metricNameWithInvalidCharacters);

		Assert.AreEqual($"diagnosticcontext_{metricName}_{postfix}", actualMetricFullName);
	}

	[TestMethod]
	public void BuildFullName_MetricNameHasNumbers_ReturnMetricNameWithNumbers()
	{
		const string metricName = "metric_name_v3";
		const string postfix = "v11_postfix";

		var actualMetricFullName = new PrometheusMetricNameBuilder(postfix: postfix)
			.BuildFullMetricName(metricName);

		Assert.AreEqual($"diagnosticcontext_{metricName}_{postfix}", actualMetricFullName);
	}
}