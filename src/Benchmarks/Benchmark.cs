using BenchmarkDotNet.Attributes;
using Mindbox.DiagnosticContext.MetricsTypes;
using Mindbox.DiagnosticContext.Prometheus;

namespace Mindbox.DiagnosticContext.Benchmarks;

[MemoryDiagnoser]
public class DiagnosticContextBenchmarks
{
	private readonly IDiagnosticContextFactory _diagnosticContextFactory = new PrometheusDiagnosticContextFactory(
		new DefaultMetricTypesConfiguration(),
		new NullDiagnosticContextLogger()
	);

	[Benchmark]
	public void WithoutSteps()
	{
		using var diagnosticContext = _diagnosticContextFactory.CreateDiagnosticContext("test");
	}

	[Benchmark]
	public void ConsequentSteps()
	{
		using var diagnosticContext = _diagnosticContextFactory.CreateDiagnosticContext("test");

		using (diagnosticContext.Measure("TryMeasureSomething0")) ;
		using (diagnosticContext.Measure("TryMeasureSomething1")) ;
		using (diagnosticContext.Measure("TryMeasureSomething2")) ;
		using (diagnosticContext.Measure("TryMeasureSomething3")) ;
		using (diagnosticContext.Measure("TryMeasureSomething4")) ;
		using (diagnosticContext.Measure("TryMeasureSomething5")) ;
		using (diagnosticContext.Measure("TryMeasureSomething6")) ;
		using (diagnosticContext.Measure("TryMeasureSomething7")) ;
		using (diagnosticContext.Measure("TryMeasureSomething8")) ;
		using (diagnosticContext.Measure("TryMeasureSomething9")) ;
	}

	[Benchmark]
	public void NestedSteps()
	{
		using var diagnosticContext = _diagnosticContextFactory.CreateDiagnosticContext("test");

		using (diagnosticContext.Measure("TryMeasureSomething0"))
		using (diagnosticContext.Measure("TryMeasureSomething1"))
		using (diagnosticContext.Measure("TryMeasureSomething2"))
		using (diagnosticContext.Measure("TryMeasureSomething3"))
		using (diagnosticContext.Measure("TryMeasureSomething4"))
		using (diagnosticContext.Measure("TryMeasureSomething5"))
		using (diagnosticContext.Measure("TryMeasureSomething6"))
		using (diagnosticContext.Measure("TryMeasureSomething7"))
		using (diagnosticContext.Measure("TryMeasureSomething8"))
		using (diagnosticContext.Measure("TryMeasureSomething9"))
			;
	}

	[Benchmark]
	public void ComplexContext()
	{
		using var diagnosticContext = _diagnosticContextFactory.CreateDiagnosticContext("test");

		using (diagnosticContext.Measure("Step1"))
		{
			using (diagnosticContext.Measure("Step1Substep1"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}

			using (diagnosticContext.Measure("Step1Substep2"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}

			using (diagnosticContext.Measure("Step1Substep3"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}
		}


		using (diagnosticContext.Measure("Step2"))
		{
			using (diagnosticContext.Measure("Step2Substep1"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}

			using (diagnosticContext.Measure("Step2Substep2"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}

			using (diagnosticContext.Measure("Step2Substep3"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}
		}


		using (diagnosticContext.Measure("Step3"))
		{
			using (diagnosticContext.Measure("Step3Substep1"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}

			using (diagnosticContext.Measure("Step3Substep2"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}

			using (diagnosticContext.Measure("Step3Substep3"))
			{
				using (diagnosticContext.Measure("Abracadabra")) ;
				using (diagnosticContext.Measure("Babracadabra")) ;
				using (diagnosticContext.Measure("Cabracadabra")) ;
				using (diagnosticContext.Measure("Dabracadabra")) ;
				using (diagnosticContext.Measure("Eabracadabra")) ;
			}
		}
	}

}