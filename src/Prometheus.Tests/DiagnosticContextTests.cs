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

using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mindbox.DiagnosticContext;
using Mindbox.DiagnosticContext.MetricsTypes;
using Mindbox.DiagnosticContext.Prometheus;

namespace Prometheus.Tests;

public class TestDateTimeAccessor : ICurrentTimeAccessor
{
	public DateTime CurrentDateTimeUtc { get; set; }
}

public abstract class DiagnosticContextTestsBase
{
	protected CollectorRegistry _metricsRegistry = null!;
	protected PrometheusDiagnosticContextFactory _factory = null!;
	protected TestDateTimeAccessor _currentTimeAccessor = null!;
	protected DefaultMetricTypesConfiguration _defaultMetricTypesConfiguration = null!;

	protected abstract PrometheusDiagnosticContextFactory CreateFactory(
		DefaultMetricTypesConfiguration config,
		IDiagnosticContextLogger logger,
		IMetricFactory metricFactory);

	[TestInitialize]
	public void TestInitialize()
	{
		_metricsRegistry = Metrics.NewCustomRegistry();
		_currentTimeAccessor = new TestDateTimeAccessor()
		{
			CurrentDateTimeUtc = new DateTime(2021, 02, 03, 04, 05, 06, 07, DateTimeKind.Utc)
		};
		_defaultMetricTypesConfiguration = new DefaultMetricTypesConfiguration(_currentTimeAccessor);
		_factory = CreateFactory(
			_defaultMetricTypesConfiguration,
			new NullDiagnosticContextLogger(),
			Metrics.WithCustomRegistry(_metricsRegistry));
	}

	[TestMethod]
	public async System.Threading.Tasks.Task MeasureWithInnerSpanAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			using (diagnosticContext.Measure("Span"))
			{
				_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
				using (diagnosticContext.Measure("InnerSpan"))
				{
					_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(2);
				}
			}
			_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(3);
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_processingtime_count 1",
			"diagnosticcontext_test_processingtime_total 6",
			"diagnosticcontext_test_processingtime{step=\"Span\",unit=\"[ms]\"} 1",
			"diagnosticcontext_test_processingtime{step=\"Span/InnerSpan\",unit=\"[ms]\"} 2",
			"diagnosticcontext_test_processingtime{step=\"Other\",unit=\"[ms]\"} 3");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task MeasureWithInnerSpanAndTagAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.SetTag("tag", "tagValue");
			using (diagnosticContext.Measure("Span"))
			{
				_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
				using (diagnosticContext.Measure("InnerSpan"))
				{
					_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(2);
				}
			}

			_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(3);
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_processingtime_count{tag=\"tagValue\"} 1",
			"diagnosticcontext_test_processingtime_total{tag=\"tagValue\"} 6",
			"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Span\",unit=\"[ms]\"} 1",
			"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Span/InnerSpan\",unit=\"[ms]\"} 2",
			"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Other\",unit=\"[ms]\"} 3");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task TwoMeasuresMeasureWithSameTagAsync()
	{
		void Measure(int milliseconds)
		{
			using var diagnosticContext = CreateDiagnosticContext("Test");

			diagnosticContext.SetTag("tag", "tagValue");
			using (diagnosticContext.Measure("Span"))
			{
				_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(milliseconds);
			}
		}

		Measure(1);
		Measure(2);
		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_processingtime_count{tag=\"tagValue\"} 2",
			"diagnosticcontext_test_processingtime_total{tag=\"tagValue\"} 3",
			"diagnosticcontext_test_processingtime{tag=\"tagValue\",step=\"Span\",unit=\"[ms]\"} 3");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task TwoMeasuresMeasureWithDifferentTagsAsync()
	{
		void Measure(string tagValue, int milliseconds)
		{
			using var diagnosticContext = CreateDiagnosticContext("Test");

			diagnosticContext.SetTag("tag", tagValue);
			using (diagnosticContext.Measure("Span"))
			{
				_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(milliseconds);
			}
		}

		Measure("tagValue1", 1);
		Measure("tagValue2", 2);
		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_processingtime_count{tag=\"tagValue1\"} 1",
			"diagnosticcontext_test_processingtime_total{tag=\"tagValue1\"} 1",
			"diagnosticcontext_test_processingtime{tag=\"tagValue1\",step=\"Span\",unit=\"[ms]\"} 1",
			"diagnosticcontext_test_processingtime_count{tag=\"tagValue2\"} 1",
			"diagnosticcontext_test_processingtime_total{tag=\"tagValue2\"} 2",
			"diagnosticcontext_test_processingtime{tag=\"tagValue2\",step=\"Span\",unit=\"[ms]\"} 2");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task ZeroSpansAreNotReportedAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			using (diagnosticContext.Measure("Span"))
			{
				_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
				using (diagnosticContext.Measure("InnerSpan"))
				{
				}
			}
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_processingtime_count 1",
			"diagnosticcontext_test_processingtime_total 1",
			"diagnosticcontext_test_processingtime{step=\"Span\",unit=\"[ms]\"} 1");
		await AssertMetricsNotReportedAsync(
			"diagnosticcontext_test_processingtime{step=\"Span/InnerSpan\",unit=\"[ms]\"}",
			"diagnosticcontext_test_processingtime{step=\"Other\",unit=\"[ms]\"}");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task MeasureForAdditionalMetric_OneLinkedDiagnosticContext_CorrectMetrics()
	{
		var diagnosticContext = CreateDiagnosticContext("Main_DC");

		using (diagnosticContext.Measure("Span"))
		{
			using (diagnosticContext.MeasureForAdditionalMetric(CreateDiagnosticContext("Additional_DC")))
			{
				using (diagnosticContext.Measure("InnerSpan"))
				{
					_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(10);
				}
			}
			_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(5);
		}

		using (diagnosticContext.Measure("FinalSpan"))
		{
			_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(15);
		}

		diagnosticContext.Dispose();

		await AssertMetricsReportedAsync(
			"diagnosticcontext_main_dc_processingtime_total 30",
			"diagnosticcontext_main_dc_processingtime_count 1",
			"diagnosticcontext_main_dc_processingtime{step=\"FinalSpan\",unit=\"[ms]\"} 15",
			"diagnosticcontext_main_dc_processingtime{step=\"Span/InnerSpan\",unit=\"[ms]\"} 10",
			"diagnosticcontext_main_dc_processingtime{step=\"Span\",unit=\"[ms]\"} 5",
			"diagnosticcontext_additional_dc_processingtime{step=\"InnerSpan\",unit=\"[ms]\"} 10",
			"diagnosticcontext_additional_dc_processingtime_count 1",
			"diagnosticcontext_additional_dc_processingtime_total 10");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task Increment_Once_WithoutTagsAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.Increment("TestCounter");
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_counters{name=\"TestCounter\"} 1");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task Increment_Once_WithTagAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.SetTag("tag", "tagValue");
			diagnosticContext.Increment("TestCounter");
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_counters{name=\"TestCounter\",tag=\"tagValue\"} 1");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task Increment_Twice_WithoutTagsAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.Increment("TestCounter");
		}

		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.Increment("TestCounter");
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_counters{name=\"TestCounter\"} 2");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task Increment_Twice_WithSameTagAsync()
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

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_counters{name=\"TestCounter\",tag=\"tagValue\"} 2");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task Increment_Twice_WithDifferentTagsAsync()
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

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_counters{name=\"TestCounter\",tag=\"tagValue1\"} 1",
			"diagnosticcontext_test_counters{name=\"TestCounter\",tag=\"tagValue2\"} 1");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task ReportValue_Once_WithoutTagsAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.ReportValue("TestValue", 1);
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\"} 1",
			"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\"} 1");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task ReportValue_Once_WithTagAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.SetTag("tag", "tagValue");
			diagnosticContext.ReportValue("TestValue", 1);
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\",tag=\"tagValue\"} 1",
			"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\",tag=\"tagValue\"} 1");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task ReportValue_Twice_WithoutTagsAsync()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.ReportValue("TestValue", 2);
		}

		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.ReportValue("TestValue", 1);
		}

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\"} 3",
			"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\"} 2");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task ReportValue_Twice_WithSameTagAsync()
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

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\",tag=\"tagValue\"} 3",
			"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\",tag=\"tagValue\"} 2");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task ReportValue_Twice_WithDifferentTagsAsync()
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

		await AssertMetricsReportedAsync(
			"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\",tag=\"tagValue1\"} 1",
			"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\",tag=\"tagValue1\"} 1",
			"diagnosticcontext_test_reportedvalues_total{name=\"TestValue\",tag=\"tagValue2\"} 1",
			"diagnosticcontext_test_reportedvalues_count{name=\"TestValue\",tag=\"tagValue2\"} 1");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task InternalMetrics_ProcessingTime()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.SetTag("tag", "tagValue");
			using (diagnosticContext.Measure("Span"))
			{
				_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
				using (diagnosticContext.Measure("InnerSpan"))
				{
					_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(2);
				}
			}

			_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(3);
		}

		await AssertMetricsReportedAsync("diagnosticcontext_test_internalprocessingtime{tag=\"tagValue\"}");
	}

	[TestMethod]
	public async System.Threading.Tasks.Task InternalMetrics_LayersCount()
	{
		using (var diagnosticContext = CreateDiagnosticContext("Test"))
		{
			diagnosticContext.SetTag("tag", "tagValue");
			using (diagnosticContext.Measure("Span"))
			{
				_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
				using (diagnosticContext.Measure("InnerSpan"))
				{
					_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(2);
				}
			}

			_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(3);
		}

		await AssertMetricsReportedAsync("diagnosticcontext_test_processingtime_layerscount{tag=\"tagValue\"} 3");
	}

	protected async System.Threading.Tasks.Task AssertMetricsReportedAsync(params string[] expectedMetrics)
	{
		var metrics = await GetMetricsAsTextAsync();

		var notFoundMetrics = expectedMetrics
			.Where(metric => !metrics.Contains(metric))
			.ToArray();

		if (notFoundMetrics.Any())
		{
			Assert.Fail(
				$"Following metrics where not reported:\r\n{string.Join("\r\n", notFoundMetrics)}\r\n\r\n" +
				$"Full metrics:\r\n{metrics}");
		}
	}

	protected async System.Threading.Tasks.Task AssertMetricsNotReportedAsync(params string[] expectedMetrics)
	{
		var metrics = await GetMetricsAsTextAsync();

		var foundMetrics = expectedMetrics
			.Where(metrics.Contains)
			.ToArray();

		if (foundMetrics.Any())
		{
			Assert.Fail(
				$"Following metrics where reported:\r\n{string.Join("\r\n", foundMetrics)}\r\n\r\n" +
				$"Full metrics:\r\n{metrics}");
		}
	}

	protected IDiagnosticContext CreateDiagnosticContext(string metricPath)
	{
		var onlyWallClockMetricTypes = _defaultMetricTypesConfiguration
			.GetDefaultMetricsTypes()
			.MetricsTypes
			.ToArray();
		return _factory.CreateDiagnosticContext(metricPath, metricsTypesOverride: onlyWallClockMetricTypes);
	}

	protected async System.Threading.Tasks.Task<string> GetMetricsAsTextAsync()
	{
		using var memoryStream = new MemoryStream();
		await _metricsRegistry.CollectAndExportAsTextAsync(memoryStream);
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}
}

[TestClass]
public class PlainDiagnosticContextTests : DiagnosticContextTestsBase
{
	protected override PrometheusDiagnosticContextFactory CreateFactory(
		DefaultMetricTypesConfiguration config,
		IDiagnosticContextLogger logger,
		IMetricFactory metricFactory)
		=> new(config, logger, metricFactory);
}

[TestClass]
public class ManagedLifetimeDiagnosticContextTests : DiagnosticContextTestsBase
{
	protected override PrometheusDiagnosticContextFactory CreateFactory(
		DefaultMetricTypesConfiguration config,
		IDiagnosticContextLogger logger,
		IMetricFactory metricFactory)
		=> new(config, logger, TimeSpan.FromMinutes(5), metricFactory);

	[TestMethod]
	public async System.Threading.Tasks.Task SeriesEvictedAfterTtlAsync()
	{
		var registry = Metrics.NewCustomRegistry();
		var factory = new PrometheusDiagnosticContextFactory(
			_defaultMetricTypesConfiguration,
			new NullDiagnosticContextLogger(),
			metricLifetime: TimeSpan.FromMilliseconds(100),
			metricFactory: Metrics.WithCustomRegistry(registry));

		var metricsTypes = _defaultMetricTypesConfiguration
			.GetDefaultMetricsTypes()
			.MetricsTypes
			.ToArray();

		using (var dc = factory.CreateDiagnosticContext("Evict", metricsTypesOverride: metricsTypes))
		{
			dc.SetTag("tag", "ephemeral");
			using (dc.Measure("Span"))
				_currentTimeAccessor.CurrentDateTimeUtc = _currentTimeAccessor.CurrentDateTimeUtc.AddMilliseconds(1);
		}

		async System.Threading.Tasks.Task<string> Collect()
		{
			using var ms = new MemoryStream();
			await registry.CollectAndExportAsTextAsync(ms);
			return Encoding.UTF8.GetString(ms.ToArray());
		}

		var before = await Collect();
		Assert.IsTrue(before.Contains("tag=\"ephemeral\""),
			$"Should be present immediately.\n{before}");

		await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(2000));

		var after = await Collect();
		Assert.IsFalse(after.Contains("tag=\"ephemeral\""),
			$"Should be evicted after TTL.\n{after}");
	}
}
