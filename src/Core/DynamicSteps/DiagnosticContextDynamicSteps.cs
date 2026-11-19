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
using Mindbox.DiagnosticContext.MetricItem;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext.DynamicSteps;

public class DiagnosticContextDynamicSteps
{
	// Only the names of the open steps are kept, with the root first: the path of a step is built when the step is
	// disposed. Keeping a path per open step would retain memory quadratic in the number of steps whenever steps
	// stay open, for example when they are never disposed.
	private readonly List<string> _openSteps = new();
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
		// An invalid context publishes nothing and never pops its open steps, so a new step would only add to the
		// memory retained until the context is disposed.
		if (IsInInvalidState)
			return new FakeTimer();

		try
		{
			if (string.IsNullOrWhiteSpace(stepName))
				throw new ArgumentException($"string.IsNullOrWhiteSpace({nameof(stepName)})");

			if (stepName.Contains('/'))
				throw new ArgumentException($"{nameof(stepName)}.Contains('/')")
				{
					Data = {{"stepName", stepName}}
				};

			_openSteps.Add(stepName);

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
			if (_openSteps[^1] != stepName)
				throw new InvalidOperationException($"Шаг {stepName} закончился раньше чем должен был. " +
					$"Текущий стек шагов: {string.Join("/", _openSteps)}. " +
					"Возможно ошибка связана с отсутствием вызова Dispose у одного из вложенных шагов.");

			_metricsValues.SetStepMetricsValues(BuildCurrentStepPath(), mc);

			_openSteps.RemoveAt(_openSteps.Count - 1);
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
			if (_openSteps.Count > 0)
				throw new InvalidOperationException("Профилирование закончилось раньше чем один из шагов. " +
					$"Текущий стек шагов: {string.Join("/", _openSteps)}. " +
					"Возможно ошибка связана с отсутствием вызова Dispose у одного из вложенных шагов.");

			_metricsValues.SetTotal(mc);
		});
	}

	// Runs on every step disposal, so it allocates only the resulting string.
	private string BuildCurrentStepPath()
	{
		if (_openSteps.Count == 1)
			return _openSteps[0];

		var length = _openSteps.Count - 1;
		for (var i = 0; i < _openSteps.Count; i++)
			length += _openSteps[i].Length;

		return string.Create(length, _openSteps, static (path, steps) =>
		{
			var position = 0;
			for (var i = 0; i < steps.Count; i++)
			{
				if (i > 0)
					path[position++] = '/';

				steps[i].AsSpan().CopyTo(path[position..]);
				position += steps[i].Length;
			}
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