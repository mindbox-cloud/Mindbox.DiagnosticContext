using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mindbox.DiagnosticContext;
using Mindbox.DiagnosticContext.MetricsTypes;
using Mindbox.DiagnosticContext.Prometheus;

namespace Prometheus.Tests
{
	public class TestDateTimeAccessor : ICurrentTimeAccessor
	{
		public DateTime CurrentDateTimeUtc { get; set; }
	}

	[TestClass]
	public class DiagnosticContextTests 
	{
		private CollectorRegistry metricsRegistry = null!;
		private PrometheusDiagnosticContextFactory factory = null!;
		private TestDateTimeAccessor currentTimeAccessor = null!;
		private DefaultMetricTypesConfiguration defaultMetricTypesConfiguration = null!;


		[TestInitialize]
		public void TestInitialize()
		{
			metricsRegistry = Metrics.NewCustomRegistry();
			currentTimeAccessor = new TestDateTimeAccessor() {CurrentDateTimeUtc = new DateTime(2021, 02, 03, 04, 05, 06, 07, DateTimeKind.Utc)};
			defaultMetricTypesConfiguration = new DefaultMetricTypesConfiguration(currentTimeAccessor);
			factory = new PrometheusDiagnosticContextFactory(defaultMetricTypesConfiguration, new NullDiagnosticContextLogger(), Metrics.WithCustomRegistry(metricsRegistry));
		}

		[TestMethod]
		public void MeasureWithInnerSpan()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				using (diagnosticContext.Measure("Span"))
				{
					currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
					using (diagnosticContext.Measure("InnerSpan"))
					{
						currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(2);
					}
				}
				currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(3);
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
					currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
					using (diagnosticContext.Measure("InnerSpan"))
					{
						currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(2);
					}
				}

				currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(3);
			}

			
			AssertMetricsReported(
				"diagnosticcontext_test_processingtime_count{tag=\"tagValue\"} 1",
				"diagnosticcontext_test_processingtime_total{tag=\"tagValue\"} 6",
				"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Span\",unit=\"[ms]\"} 1",
				"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Span/InnerSpan\",unit=\"[ms]\"} 2",
				"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Other\",unit=\"[ms]\"} 3");
		}

		[TestMethod]
		public void TwoMeasuresMeasureWithSameTag()
		{
			void Measure(int milliseconds)
			{
				using var diagnosticContext = CreateDiagnosticContext("Test");
				
				diagnosticContext.SetTag("tag", "tagValue");
				using (diagnosticContext.Measure("Span"))
				{
					currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(milliseconds);
				}
			}


			Measure(1);
			Measure(2);

			
			AssertMetricsReported(
				"diagnosticcontext_test_processingtime_count{tag=\"tagValue\"} 2",
				"diagnosticcontext_test_processingtime_total{tag=\"tagValue\"} 3",
				"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Span\",unit=\"[ms]\"} 3");
		}

		[TestMethod]
		public void TwoMeasuresMeasureWithDifferentTags()
		{
			void Measure(string tagValue, int milliseconds)
			{
				using var diagnosticContext = CreateDiagnosticContext("Test");
				
				diagnosticContext.SetTag("tag", tagValue);
				using (diagnosticContext.Measure("Span"))
				{
					currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(milliseconds);
				}
			}


			Measure("tagValue1", 1);
			Measure("tagValue2", 2);

			
			AssertMetricsReported(
				"diagnosticcontext_test_processingtime_count{tag=\"tagValue1\"} 1",
				"diagnosticcontext_test_processingtime_total{tag=\"tagValue1\"} 1",
				"diagnosticcontext_test_processingtime{tag=\"tagValue1\",step=\"Span\",unit=\"[ms]\"} 1",
				"diagnosticcontext_test_processingtime_count{tag=\"tagValue2\"} 1",
				"diagnosticcontext_test_processingtime_total{tag=\"tagValue2\"} 2",
				"diagnosticcontext_test_processingtime{tag=\"tagValue2\",step=\"Span\",unit=\"[ms]\"} 2");
		}
		
		[TestMethod]
		public void ZeroSpansAreNotReported()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				using (diagnosticContext.Measure("Span"))
				{
					currentTimeAccessor.CurrentDateTimeUtc = currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
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
		
		[TestMethod]
		public void Increment_Once_WithoutTags()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.Increment("TestCounter");
			}

			AssertMetricsReported(
				"diagnosticcontext_test_counters{name=\"TestCounter\"} 1");
		}
		
		[TestMethod]
		public void Increment_Once_WithTag()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue");
				diagnosticContext.Increment("TestCounter");
			}

			AssertMetricsReported(
				"diagnosticcontext_test_counters{name=\"TestCounter\",tag=\"tagValue\"} 1");
		}
		
		[TestMethod]
		public void Increment_Twice_WithoutTags()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.Increment("TestCounter");
			}
			
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.Increment("TestCounter");
			}

			AssertMetricsReported(
				"diagnosticcontext_test_counters{name=\"TestCounter\"} 2");
		}
		
		[TestMethod]
		public void Increment_Twice_WithSameTag()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue");
				diagnosticContext.Increment("TestCounter");
			}
			
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue");
				diagnosticContext.Increment("TestCounter");
			}

			AssertMetricsReported(
				"diagnosticcontext_test_counters{name=\"TestCounter\",tag=\"tagValue\"} 2");
		}
		
		[TestMethod]
		public void Increment_Twice_WithDifferentTags()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue1");
				diagnosticContext.Increment("TestCounter");
			}
			
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue2");
				diagnosticContext.Increment("TestCounter");
			}

			AssertMetricsReported(
				"diagnosticcontext_test_counters{name=\"TestCounter\",tag=\"tagValue1\"} 1",
				"diagnosticcontext_test_counters{name=\"TestCounter\",tag=\"tagValue2\"} 1");
		}

		[TestMethod]
		public void ReportValue_Once_WithoutTags()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.ReportValue("TestValue", 1);
			}

			AssertMetricsReported(
				"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\"} 1",
				"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\"} 1");
		}
		
		[TestMethod]
		public void ReportValue_Once_WithTag()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue");
				diagnosticContext.ReportValue("TestValue", 1);
			}

			AssertMetricsReported(
				"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\",tag=\"tagValue\"} 1",
				"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\",tag=\"tagValue\"} 1");
		}
		
		[TestMethod]
		public void ReportValue_Twice_WithoutTags()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.ReportValue("TestValue", 2);
			}
			
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.ReportValue("TestValue", 1);
			}
			
			AssertMetricsReported(
				"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\"} 3",
				"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\"} 2");
		}
		
		[TestMethod]
		public void ReportValue_Twice_WithSameTag()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue");
				diagnosticContext.ReportValue("TestValue", 2);
			}
			
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue");
				diagnosticContext.ReportValue("TestValue", 1);
			}

			AssertMetricsReported(
				"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\",tag=\"tagValue\"} 3",
				"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\",tag=\"tagValue\"} 2");
		}
		
		[TestMethod]
		public void ReportValue_Twice_WithDifferentTags()
		{
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue1");
				diagnosticContext.ReportValue("TestValue", 1);
			}
			
			using (var diagnosticContext = CreateDiagnosticContext("Test"))
			{
				diagnosticContext.SetTag("tag", "tagValue2");
				diagnosticContext.ReportValue("TestValue", 1);
			}

			AssertMetricsReported(
				"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\",tag=\"tagValue1\"} 1",
				"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\",tag=\"tagValue1\"} 1",
				"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\",tag=\"tagValue2\"} 1",
				"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\",tag=\"tagValue2\"} 1");
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
					$"Following metrics where not reported:\r\n{String.Join("\r\n", notFoundMetrics)}\r\n\r\n" +
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
					$"Following metrics where reported:\r\n{String.Join("\r\n", foundMetrics)}\r\n\r\n" +
					$"Full metrics:\r\n{metrics}");
			}
		}

		private IDiagnosticContext CreateDiagnosticContext(string metricPath)
		{
			var onlyWallClockMetricTypes = defaultMetricTypesConfiguration
				.GetAsyncMetricsTypes()
				.MetricsTypes
				.ToArray();
			return factory.CreateDiagnosticContext(metricPath, metricsTypesOverride: onlyWallClockMetricTypes);
		}

		private string GetMetricsAsText()
		{
			using var memoryStream = new MemoryStream();
			metricsRegistry.CollectAndExportAsTextAsync(memoryStream).GetAwaiter().GetResult();
			return Encoding.UTF8.GetString(memoryStream.ToArray());
		}
	}
}