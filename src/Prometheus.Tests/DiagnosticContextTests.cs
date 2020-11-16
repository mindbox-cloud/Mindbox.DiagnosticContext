using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Itc.Commons;
using Itc.Commons.Model;
using Itc.Commons.Tests.Infrastructure;
using Itc.Commons.Tests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mindbox.DiagnosticContext.Prometheus;

namespace Prometheus.Tests
{
	[TestClass]
	public class DiagnosticContextTests : CommonsTests
	{
		private readonly CollectorRegistry metricsRegistry = Metrics.NewCustomRegistry();
		
		[TestMethod]
		public void MeasureWithInnerSpan()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				using (diagnosticContext.Measure("Span"))
				{
					Controller.CurrentDateTimeUtc = Controller.CurrentDateTimeUtc.AddMilliseconds(1);
					using (diagnosticContext.Measure("InnerSpan"))
					{
						Controller.CurrentDateTimeUtc = Controller.CurrentDateTimeUtc.AddMilliseconds(2);
					}
				}
				Controller.CurrentDateTimeUtc = Controller.CurrentDateTimeUtc.AddMilliseconds(3);
			}


			AssertMetricsReported(
				"diagnosticcontext_test_processingtime_count 1",
				"diagnosticcontext_test_processingtime_total 6",
				"diagnosticcontext_test_processingtime{step=\"Span\",unit=\"[ms]\"} 1",
				"diagnosticcontext_test_processingtime{step=\"Span/InnerSpan\",unit=\"[ms]\"} 2",
				"diagnosticcontext_test_processingtime{step=\"Other\",unit=\"[ms]\"} 3");
		}

		[TestMethod]
		public void MeasureWithInnerSpanAndTag()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue");
				using (diagnosticContext.Measure("Span"))
				{
					Controller.CurrentDateTimeUtc = Controller.CurrentDateTimeUtc.AddMilliseconds(1);
					using (diagnosticContext.Measure("InnerSpan"))
					{
						Controller.CurrentDateTimeUtc = Controller.CurrentDateTimeUtc.AddMilliseconds(2);
					}
				}

				Controller.CurrentDateTimeUtc = Controller.CurrentDateTimeUtc.AddMilliseconds(3);
			}

			
			AssertMetricsReported(
				"diagnosticcontext_test_processingtime_count{tag=\"tagValue\"} 1",
				"diagnosticcontext_test_processingtime_total{tag=\"tagValue\"} 6",
				"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Span\",unit=\"[ms]\"} 1",
				"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Span/InnerSpan\",unit=\"[ms]\"} 2",
				"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Other\",unit=\"[ms]\"} 3");
		}
		
		[TestMethod]
		public void ZeroSpansAreNotReported()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				using (diagnosticContext.Measure("Span"))
				{
					Controller.CurrentDateTimeUtc = Controller.CurrentDateTimeUtc.AddMilliseconds(1);
					using (diagnosticContext.Measure("InnerSpan"))
					{
					}
				}
			}


			AssertMetricsReported(
				"diagnosticcontext_test_processingtime_count 1",
				"diagnosticcontext_test_processingtime_total 1",
				"diagnosticcontext_test_processingtime{step=\"Span\",unit=\"[ms]\"} 1");


			AssertMetricsNotReported(
				"diagnosticcontext_test_processingtime{step=\"Span/InnerSpan\",unit=\"[ms]\"}",
				"diagnosticcontext_test_processingtime{step=\"Other\",unit=\"[ms]\"}");
		}

		private void AssertMetricsReported(params string[] expectedMetrics)
		{
			var metrics = GetMetricsAsText();

			var notFoundMetrics = expectedMetrics
				.Where(metric => !metrics.Contains(metric))
				.ToArray();

			if (notFoundMetrics.Any())
			{
				Assert.Fail(
					$"Following metrics where not reported:\r\n{notFoundMetrics.StringJoin("\r\n")}\r\n\r\n" +
					$"Full metrics:\r\n{metrics}");
			}
		}

		private void AssertMetricsNotReported(params string[] expectedMetrics)
		{
			var metrics = GetMetricsAsText();

			var foundMetrics = expectedMetrics
				.Where(metric => metrics.Contains(metric))
				.ToArray();

			if (foundMetrics.Any())
			{
				Assert.Fail(
					$"Following metrics where reported:\r\n{foundMetrics.StringJoin("\r\n")}\r\n\r\n" +
					$"Full metrics:\r\n{metrics}");
			}
		}

		private IDiagnosticContext CreateDiagnosticContext(string metricPath)
		{
			var factory = new PrometheusDiagnosticContextFactory(Metrics.WithCustomRegistry(metricsRegistry));
			return factory.CreateDiagnosticContext(metricPath);
		}

		private string GetMetricsAsText()
		{
			using var memoryStream = new MemoryStream();
			metricsRegistry.CollectAndExportAsTextAsync(memoryStream).GetAwaiter().GetResult();
			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}
	}
}