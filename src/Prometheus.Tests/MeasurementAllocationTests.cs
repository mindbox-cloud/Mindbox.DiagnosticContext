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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mindbox.DiagnosticContext;
using Mindbox.DiagnosticContext.MetricsTypes;
using Mindbox.DiagnosticContext.Prometheus;

namespace Prometheus.Tests;

/// <summary>
/// Allocation budgets of the measurement hot path. Every check measures a batch after a warm-up batch, so JIT
/// and type loading are not counted, and uses limits with headroom for other runtimes and platforms.
/// </summary>
[TestClass]
public class MeasurementAllocationTests
{
	private const int WarmUpSteps = 1_000;
	private const int MeasuredSteps = 1_000;

	// A cost that grows with the number of earlier steps makes the measured batch exceed its limit about ten
	// times, while the earlier steps stay cheap enough for a test run even when that cost returns.
	private const int EarlierStepsOfBrokenContext = 4_000;

	// A healthy flat step allocates about 250 B; with per-step closures and adapters it allocated about 1.9 KB.
	private const long HealthyStepBytesLimit = 1_024;

	// This limit checks that the cost of a step does not depend on the earlier steps, not the cost itself:
	// a constant cost of up to about 4 KB per step passes.
	private const long BrokenContextBatchBytesLimit = 4 * 1_024 * 1_024;

	[TestMethod]
	public void HealthyStepStaysWithinAllocationBudget()
	{
		using var context = CreateFactory().CreateDiagnosticContext("Allocations");

		RunDisposedSteps(context, WarmUpSteps);
		var bytesPerStep = MeasureAllocatedBytes(() => RunDisposedSteps(context, MeasuredSteps)) / MeasuredSteps;

		Assert.IsTrue(
			bytesPerStep <= HealthyStepBytesLimit,
			$"A step allocated {bytesPerStep} B, the limit is {HealthyStepBytesLimit} B.");
	}

	[TestMethod]
	public void DisabledContextStepDoesNotAllocate()
	{
		using var context = new NullDiagnosticContext();

		RunDisposedSteps(context, WarmUpSteps);
		var allocatedBytes = MeasureAllocatedBytes(() => RunDisposedSteps(context, MeasuredSteps));

		Assert.AreEqual(0, allocatedBytes, $"{MeasuredSteps} steps allocated {allocatedBytes} B.");
	}

	[TestMethod]
	public void StepCostDoesNotGrowAfterInvalidStepName()
	{
		using var context = CreateFactory().CreateDiagnosticContext("InvalidName");

		context.Measure("Invalid/Name").Dispose();

		AssertStepCostDoesNotGrow(() => RunDisposedSteps(context, EarlierStepsOfBrokenContext), () => RunDisposedSteps(context, MeasuredSteps));
	}

	[TestMethod]
	public void StepCostDoesNotGrowAfterOutOfOrderDisposal()
	{
		using var context = CreateFactory().CreateDiagnosticContext("OutOfOrder");

		var outer = context.Measure("Outer");
		var inner = context.Measure("Inner");
		outer.Dispose();
		inner.Dispose();

		AssertStepCostDoesNotGrow(() => RunDisposedSteps(context, EarlierStepsOfBrokenContext), () => RunDisposedSteps(context, MeasuredSteps));
	}

	[TestMethod]
	public void StepCostDoesNotGrowWhenStepsAreNotDisposed()
	{
		using var context = CreateFactory().CreateDiagnosticContext("NotDisposed");

		AssertStepCostDoesNotGrow(
			() => RunUndisposedSteps(context, EarlierStepsOfBrokenContext),
			() => RunUndisposedSteps(context, MeasuredSteps));
	}

	private static void AssertStepCostDoesNotGrow(Action runEarlierSteps, Action runMeasuredSteps)
	{
		runEarlierSteps();
		var allocatedBytes = MeasureAllocatedBytes(runMeasuredSteps);

		Assert.IsTrue(
			allocatedBytes <= BrokenContextBatchBytesLimit,
			$"{MeasuredSteps} steps after {EarlierStepsOfBrokenContext} earlier steps allocated {allocatedBytes} B, " +
			$"the limit is {BrokenContextBatchBytesLimit} B.");
	}

	private static void RunDisposedSteps(IDiagnosticContext context, int count)
	{
		for (var i = 0; i < count; i++)
		{
			using (context.Measure("Step"))
			{
			}
		}
	}

	private static void RunUndisposedSteps(IDiagnosticContext context, int count)
	{
		for (var i = 0; i < count; i++)
			context.Measure("Step");
	}

	private static long MeasureAllocatedBytes(Action action)
	{
		var before = GC.GetAllocatedBytesForCurrentThread();
		action();
		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	private static PrometheusDiagnosticContextFactory CreateFactory()
	{
		return new PrometheusDiagnosticContextFactory(
			new DefaultMetricTypesConfiguration(),
			new NullDiagnosticContextLogger(),
			TimeSpan.FromMinutes(5),
			Metrics.WithCustomRegistry(Metrics.NewCustomRegistry()));
	}
}
