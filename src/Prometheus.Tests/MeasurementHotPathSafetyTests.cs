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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mindbox.DiagnosticContext;
using Mindbox.DiagnosticContext.DynamicStepsAggregatedStorage;
using Mindbox.DiagnosticContext.MetricItem;
using Mindbox.DiagnosticContext.MetricsTypes;
using Mindbox.DiagnosticContext.Prometheus;

namespace Prometheus.Tests;

[TestClass]
public class MeasurementHotPathSafetyTests
{
	[TestMethod]
	public void ThrowingExceptionMessageIsIsolated()
	{
		var handler = new SafeExceptionHandler();

		Assert.IsFalse(handler.HandleExceptions(() => throw new ThrowingMessageException()));

		Assert.IsTrue(handler.IsInInvalidState);
	}

	[TestMethod]
	public void LinkedContextWithThrowingMessageLeavesMeasurementUsable()
	{
		var context = CreateFactory(Metrics.NewCustomRegistry()).CreateDiagnosticContext("Throwing");

		using (context.MeasureForAdditionalMetric(new ThrowingMessageContext()))
		{
			var failed = context.Measure("Step");
			Assert.IsNotNull(failed);
			Assert.AreSame(failed, failed.SetSpanAttribute("name", "value"));
			failed.Dispose();
			failed.Dispose();

			// The context is invalid now: later measurements must be no-op, not exceptions.
			var disabled = context.Measure("Another");
			Assert.IsNotNull(disabled);
			Assert.AreSame(disabled, disabled.SetSpanAttribute("name", "value"));
			disabled.Dispose();
			disabled.Dispose();
		}

		context.Dispose();
	}

	[TestMethod]
	public void DisabledContextMeasurementIsNotNullNoOpAndIdempotent()
	{
		using var context = new NullDiagnosticContext();

		var measurement = context.Measure("Step");
		Assert.IsNotNull(measurement);
		Assert.AreSame(measurement, measurement.SetSpanAttribute("name", "value"));

		measurement.Dispose();
		measurement.Dispose();
	}

	[TestMethod]
	public async Task LinkedMeasurementDisposalFailureCanBeRetried()
	{
		var registry = Metrics.NewCustomRegistry();
		var context = CreateFactory(registry).CreateDiagnosticContext("Retry");

		using (context.MeasureForAdditionalMetric(new RetryContext()))
		{
			var measurement = context.Measure("Step");

			Assert.ThrowsException<InvalidOperationException>(measurement.Dispose);
			measurement.Dispose();
		}

		context.Dispose();

		StringAssert.Contains(
			await DumpAsync(registry),
			"diagnosticcontext_retry_processingtime_count 1");
	}

	[TestMethod]
	public async Task NestedSameNameStepsAcrossAwaitKeepPathAndLatestTag()
	{
		var registry = Metrics.NewCustomRegistry();
		var time = new TestDateTimeAccessor { CurrentDateTimeUtc = DateTime.UtcNow };
		var factory = new PrometheusDiagnosticContextFactory(
			new DefaultMetricTypesConfiguration(time),
			new NullDiagnosticContextLogger(),
			TimeSpan.FromMinutes(1),
			Metrics.WithCustomRegistry(registry));

		using (var context = factory.CreateDiagnosticContext("Async"))
		{
			context.SetTag("tenant", "old");

			var outer = context.Measure("Same");
			await Task.Yield();
			time.CurrentDateTimeUtc = time.CurrentDateTimeUtc.AddMilliseconds(1);

			using (context.Measure("Same"))
			{
				await Task.Yield();
				time.CurrentDateTimeUtc = time.CurrentDateTimeUtc.AddMilliseconds(2);
			}

			outer.Dispose();
			outer.Dispose();
			context.SetTag("tenant", "new");
		}

		var text = await DumpAsync(registry);
		StringAssert.Contains(text, "diagnosticcontext_async_processingtime{tenant=\"new\",step=\"Same\",unit=\"[ms]\"} 1");
		StringAssert.Contains(text, "diagnosticcontext_async_processingtime{tenant=\"new\",step=\"Same/Same\",unit=\"[ms]\"} 2");
		Assert.IsFalse(text.Contains("tenant=\"old\""), text);
	}

	[TestMethod]
	public async Task SameNameStepsDisposedOutOfOrderKeepStackPathsAndValues()
	{
		var registry = Metrics.NewCustomRegistry();
		var time = new TestDateTimeAccessor { CurrentDateTimeUtc = DateTime.UtcNow };
		var factory = new PrometheusDiagnosticContextFactory(
			new DefaultMetricTypesConfiguration(time),
			new NullDiagnosticContextLogger(),
			TimeSpan.FromMinutes(1),
			Metrics.WithCustomRegistry(registry));

		using (var context = factory.CreateDiagnosticContext("OutOfOrder"))
		{
			var outer = context.Measure("Same");
			time.CurrentDateTimeUtc = time.CurrentDateTimeUtc.AddMilliseconds(1);
			var inner = context.Measure("Same");
			time.CurrentDateTimeUtc = time.CurrentDateTimeUtc.AddMilliseconds(3);
			outer.Dispose();
			using (context.Measure("Sibling"))
				time.CurrentDateTimeUtc = time.CurrentDateTimeUtc.AddMilliseconds(2);
			time.CurrentDateTimeUtc = time.CurrentDateTimeUtc.AddMilliseconds(4);
			inner.Dispose();
			using (context.Measure("Next"))
				time.CurrentDateTimeUtc = time.CurrentDateTimeUtc.AddMilliseconds(5);
		}

		var text = await DumpAsync(registry);
		StringAssert.Contains(text, "diagnosticcontext_outoforder_processingtime{step=\"Same/Same\",unit=\"[ms]\"} 4");
		StringAssert.Contains(text, "diagnosticcontext_outoforder_processingtime{step=\"Same/Sibling\",unit=\"[ms]\"} 2");
		StringAssert.Contains(text, "diagnosticcontext_outoforder_processingtime{step=\"Same\",unit=\"[ms]\"} 3");
		StringAssert.Contains(text, "diagnosticcontext_outoforder_processingtime{step=\"Next\",unit=\"[ms]\"} 5");
		StringAssert.Contains(text, "diagnosticcontext_outoforder_processingtime{step=\"Other\",unit=\"[ms]\"} 1");
		Assert.IsFalse(text.Contains("step=\"Same/Next\""), text);
	}

	[TestMethod]
	public void AggregationOverflowKeepsPartialStepState()
	{
		var metricsType = new DefaultMetricTypesConfiguration().GetDefaultMetricsTypes().MetricsTypes.First();
		var aggregated = new MetricsAggregatedValue(metricsType);
		var normalizedValue = new DiagnosticContextMetricsNormalizedValue(
			metricsType.SystemName,
			new Dictionary<string, long> { ["a"] = long.MaxValue, ["b"] = 1, ["c"] = 2 });

		Assert.ThrowsException<OverflowException>(() => aggregated.Add(normalizedValue));

		Assert.AreEqual(2L, aggregated.StepValues["c"].Total);
	}

	private static PrometheusDiagnosticContextFactory CreateFactory(CollectorRegistry registry)
	{
		return new PrometheusDiagnosticContextFactory(
			new DefaultMetricTypesConfiguration(),
			new NullDiagnosticContextLogger(),
			TimeSpan.FromMinutes(5),
			Metrics.WithCustomRegistry(registry));
	}

	private static async Task<string> DumpAsync(CollectorRegistry registry)
	{
		using var stream = new MemoryStream();
		await registry.CollectAndExportAsTextAsync(stream);
		return Encoding.UTF8.GetString(stream.ToArray());
	}

	private sealed class ThrowingMessageException : Exception
	{
		public override string Message => throw new InvalidOperationException("Message getter threw");
	}

	private sealed class ThrowingMessageContext : IDiagnosticContext
	{
		public string PrefixName => "Throwing";

		public IDisposable MeasureForAdditionalMetric(IDiagnosticContext diagnosticContext)
		{
			throw new NotSupportedException();
		}

		public IMeasurement Measure(string stepName)
		{
			throw new ThrowingMessageException();
		}

		public void SetTag(string tag, string value)
		{
		}

		public void Increment(string counterPath)
		{
		}

		public IDisposable ExtendCodeSourceLabel(string extensionCodeSourceLabel)
		{
			throw new NotSupportedException();
		}

		public void ReportValue(string counterPath, long value)
		{
		}

		public void Dispose()
		{
		}
	}

	private sealed class RetryContext : IDiagnosticContext
	{
		public string PrefixName => "Retry";

		public IDisposable MeasureForAdditionalMetric(IDiagnosticContext diagnosticContext)
		{
			throw new NotSupportedException();
		}

		public IMeasurement Measure(string stepName)
		{
			return new RetryMeasurement();
		}

		public void SetTag(string tag, string value)
		{
		}

		public void Increment(string counterPath)
		{
		}

		public IDisposable ExtendCodeSourceLabel(string extensionCodeSourceLabel)
		{
			throw new NotSupportedException();
		}

		public void ReportValue(string counterPath, long value)
		{
		}

		public void Dispose()
		{
		}
	}

	private sealed class RetryMeasurement : IMeasurement
	{
		private bool _failed;

		public IMeasurement SetSpanAttribute(string attribute, string value)
		{
			return this;
		}

		public void Dispose()
		{
			if (_failed)
				return;

			_failed = true;

			throw new InvalidOperationException("first dispose threw");
		}
	}
}
