#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.MetricItem;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext.DynamicSteps
{
	public class DiagnosticContextDynamicSteps
	{
		private readonly Stack<string> stepsStack = new Stack<string>();
		private readonly SafeExceptionHandler safeExceptionHandler = new SafeExceptionHandler();
		private readonly DiagnosticContextMetricsHierarchicalValueCollection metricsValues;
		private readonly MetricsTypeCollection metricsTypes;

		internal DiagnosticContextDynamicSteps(MetricsTypeCollection metricsTypes)
		{
			this.metricsTypes = metricsTypes;
			metricsValues = DiagnosticContextMetricsHierarchicalValueCollection.FromMetricsTypeCollection(metricsTypes);
		}

		internal bool IsInInvalidState => safeExceptionHandler.IsInInvalidState;

		internal IDisposable StartStep(string stepName)
		{
			return safeExceptionHandler.HandleExceptions<IDisposable>(
				() =>
				{
					if (string.IsNullOrWhiteSpace(stepName))
						throw new ArgumentException($"string.IsNullOrWhiteSpace({nameof(stepName)})");

					if (stepName.Contains('/'))
						throw new ArgumentException($"{nameof(stepName)}.Contains('/')");

					stepsStack.Push(stepName);

					return new DisposableAdapter<MetricsMeasurerCollection>(
						metricsTypes.CreateMeasurers(),
						mc => mc.Start(),
						mc => ProcessMeasurements(stepName, mc));
				},
				() => new FakeTimer());
		}

		internal IDisposable StartTotal()
		{
			return safeExceptionHandler.HandleExceptions<IDisposable>(
				() => new DisposableAdapter<MetricsMeasurerCollection>(
					metricsTypes.CreateMeasurers(),
					mc => mc.Start(),
					ProcessTotal), 
				() => new FakeTimer());
		}

		private void ProcessMeasurements(
			string stepName,
			MetricsMeasurerCollection mc)
		{
			safeExceptionHandler.HandleExceptions(() =>
			{
				mc.Stop();
			});

			if (IsInInvalidState)
				return;

			safeExceptionHandler.HandleExceptions(() =>
			{
				if (stepsStack.Peek() != stepName)
					throw new InvalidOperationException($"Шаг {stepName} закончился раньше чем должен был. " +
						$"Текущий стек шагов: {string.Join("/", stepsStack.Reverse())}. " +
						"Возможно ошибка связана с отсутствием вызова Dispose у одного из вложенных шагов.");

				var stepPath = string.Join("/", stepsStack.Reverse());

				metricsValues.SetStepMetricsValues(stepPath, mc);

				stepsStack.Pop();
			});
		}

		private void ProcessTotal(MetricsMeasurerCollection mc)
		{
			safeExceptionHandler.HandleExceptions(() =>
			{
				mc.Stop();
			});

			if (IsInInvalidState)
				return;

			safeExceptionHandler.HandleExceptions(() =>
			{
				if (stepsStack.Any())
					throw new InvalidOperationException("Профилирование закончилось раньше чем один из шагов. " +
						$"Текущий стек шагов: {string.Join("/", stepsStack.Reverse())}. " +
						"Возможно ошибка связана с отсутствием вызова Dispose у одного из вложенных шагов.");

				metricsValues.SetTotal(mc);
			});
		}

		public DiagnosticContextMetricsNormalizedValueCollection GetNormalizedMetricsValues()
		{
			return metricsValues.ToNormalizedValueCollection();
		}
	}
}
