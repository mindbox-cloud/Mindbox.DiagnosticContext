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
