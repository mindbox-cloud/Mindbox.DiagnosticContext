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
	private readonly Stack<(string Name, string Path)> _stepsStack = new();
	private readonly SafeExceptionHandler _safeExceptionHandler;
	private readonly DiagnosticContextMetricsHierarchicalValueCollection _metricsValues;
	private readonly MetricsTypeCollection _metricsTypes;

	internal DiagnosticContextDynamicSteps(MetricsTypeCollection metricsTypes, IDiagnosticContextLogger diagnosticContextLogger)
	{
		_metricsTypes = metricsTypes;
		_metricsValues = DiagnosticContextMetricsHierarchicalValueCollection
			.FromMetricsTypeCollection(metricsTypes, diagnosticContextLogger);
		_safeExceptionHandler = new SafeExceptionHandler(diagnosticContextLogger);
	}

	internal bool IsInInvalidState => _safeExceptionHandler.IsInInvalidState;

	internal IDisposable StartStep(string stepName)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(stepName))
				throw new ArgumentException($"string.IsNullOrWhiteSpace({nameof(stepName)})");

			if (stepName.Contains('/'))
				throw new ArgumentException($"{nameof(stepName)}.Contains('/')")
				{
					Data = {{"stepName", stepName}}
				};

			// The path is built once per step and reused on disposal: joining the stack again on every dispose
			// allocated an iterator and a string per step, and the string grows with the nesting depth.
			var path = _stepsStack.Count == 0 ? stepName : _stepsStack.Peek().Path + "/" + stepName;
			_stepsStack.Push((stepName, path));

			var measurers = _metricsTypes.CreateMeasurers();
			measurers.Start();

			return new StepTimer(this, measurers, stepName);
		}
		catch (Exception exception)
		{
			_safeExceptionHandler.ProcessException(exception);
			return new FakeTimer();
		}
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

	private void ProcessMeasurements(string stepName, MetricsMeasurerCollection mc)
	{
		try
		{
			mc.Stop();
		}
		catch (Exception exception)
		{
			_safeExceptionHandler.ProcessException(exception);
		}

		if (IsInInvalidState)
			return;

		try
		{
			var currentStep = _stepsStack.Peek();
			if (currentStep.Name != stepName)
				throw new InvalidOperationException($"Шаг {stepName} закончился раньше чем должен был. " +
					$"Текущий стек шагов: {string.Join("/", _stepsStack.Reverse().Select(step => step.Name))}. " +
					"Возможно ошибка связана с отсутствием вызова Dispose у одного из вложенных шагов.");

			_metricsValues.SetStepMetricsValues(currentStep.Path, mc);

			_stepsStack.Pop();
		}
		catch (Exception exception)
		{
			_safeExceptionHandler.ProcessException(exception);
		}
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
					$"Текущий стек шагов: {string.Join("/", _stepsStack.Reverse().Select(step => step.Name))}. " +
					"Возможно ошибка связана с отсутствием вызова Dispose у одного из вложенных шагов.");

			_metricsValues.SetTotal(mc);
		});
	}

	public DiagnosticContextMetricsNormalizedValueCollection GetNormalizedMetricsValues()
	{
		return _metricsValues.ToNormalizedValueCollection();
	}

	/// <summary>
	/// One open step and its measurers.
	/// </summary>
	private sealed class StepTimer : IDisposable
	{
		private readonly DiagnosticContextDynamicSteps _owner;
		private readonly MetricsMeasurerCollection _measurers;
		private readonly string _stepName;

		private bool _disposed;

		public StepTimer(
			DiagnosticContextDynamicSteps owner,
			MetricsMeasurerCollection measurers,
			string stepName)
		{
			_owner = owner;
			_measurers = measurers;
			_stepName = stepName;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			_owner.ProcessMeasurements(_stepName, _measurers);
		}
	}
}