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

namespace Mindbox.DiagnosticContext.DynamicSteps;

public class DiagnosticContextDynamicSteps
{
	private readonly Stack<string> _stepsStack = new();
	private readonly SafeExceptionHandler _safeExceptionHandler = new();
	private readonly DiagnosticContextMetricsHierarchicalValueCollection _metricsValues;
	private readonly MetricsTypeCollection _metricsTypes;

	internal DiagnosticContextDynamicSteps(MetricsTypeCollection metricsTypes, IDiagnosticContextLogger diagnosticContextLogger)
	{
		_metricsTypes = metricsTypes;
		_metricsValues = DiagnosticContextMetricsHierarchicalValueCollection
			.FromMetricsTypeCollection(metricsTypes, diagnosticContextLogger);
	}

	internal bool IsInInvalidState => _safeExceptionHandler.IsInInvalidState;

	internal IDisposable StartStep(string stepName)
	{
		return _safeExceptionHandler.HandleExceptions<IDisposable>(
			() =>
			{
				if (string.IsNullOrWhiteSpace(stepName))
					throw new ArgumentException($"string.IsNullOrWhiteSpace({nameof(stepName)})");

				if (stepName.Contains('/'))
					throw new ArgumentException($"{nameof(stepName)}.Contains('/')");

				_stepsStack.Push(stepName);

				return new DisposableAdapter<MetricsMeasurerCollection>(
					_metricsTypes.CreateMeasurers(),
					mc => mc.Start(),
					mc => ProcessMeasurements(stepName, mc));
			},
			() => new FakeTimer());
	}

	internal IDisposable StartTotal()
	{
		return _safeExceptionHandler.HandleExceptions<IDisposable>(
			() => new DisposableAdapter<MetricsMeasurerCollection>(
				_metricsTypes.CreateMeasurers(),
				mc => mc.Start(),
				ProcessTotal),
			() => new FakeTimer());
	}

	private void ProcessMeasurements(
		string stepName,
		MetricsMeasurerCollection mc)
	{
		_safeExceptionHandler.HandleExceptions(mc.Stop);

		if (IsInInvalidState)
			return;

		_safeExceptionHandler.HandleExceptions(() =>
		{
			if (_stepsStack.Peek() != stepName)
				throw new InvalidOperationException($"Шаг {stepName} закончился раньше чем должен был. " +
					$"Текущий стек шагов: {string.Join("/", _stepsStack.Reverse())}. " +
					"Возможно ошибка связана с отсутствием вызова Dispose у одного из вложенных шагов.");

			var stepPath = string.Join("/", _stepsStack.Reverse());

			_metricsValues.SetStepMetricsValues(stepPath, mc);

			_stepsStack.Pop();
		});
	}

	private void ProcessTotal(MetricsMeasurerCollection mc)
	{
		_safeExceptionHandler.HandleExceptions(mc.Stop);

		if (IsInInvalidState)
			return;

		_safeExceptionHandler.HandleExceptions(() =>
		{
			if (_stepsStack.Any())
				throw new InvalidOperationException("Профилирование закончилось раньше чем один из шагов. " +
					$"Текущий стек шагов: {string.Join("/", _stepsStack.Reverse())}. " +
					"Возможно ошибка связана с отсутствием вызова Dispose у одного из вложенных шагов.");

			_metricsValues.SetTotal(mc);
		});
	}

	public DiagnosticContextMetricsNormalizedValueCollection GetNormalizedMetricsValues()
	{
		return _metricsValues.ToNormalizedValueCollection();
	}
}