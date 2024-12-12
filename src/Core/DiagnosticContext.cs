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
using Mindbox.DiagnosticContext.MetricItem;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext;

public class DiagnosticContext : IDiagnosticContext
{
	private readonly DiagnosticContextCollection _diagnosticContextCollection = new();
	private IDiagnosticContextMetricsCollection _metricsCollection;
	private DiagnosticContextMetricsItem _metricsItem;
	private DiagnosticContextInternalMetricsItem _internalMetricsItem;
	private IDisposable _totalTimer;

	private bool _disposed;
	private readonly SafeExceptionHandler _safeExceptionHandler;

	public string PrefixName => _metricsItem?.MetricPrefix;

	private IDisposable _sourceCodeLabelScope = null;

	public DiagnosticContext(
		IDiagnosticContextLogger diagnosticContextLogger,
		string metricPath,
		IDiagnosticContextMetricsCollection metricsCollection,
		MetricsTypeCollection metricTypes,
		bool isFeatureBoundaryCodePoint = false)
	{
		_safeExceptionHandler = new SafeExceptionHandler(diagnosticContextLogger);
		_safeExceptionHandler.HandleExceptions(() =>
		{
			if (string.IsNullOrEmpty(metricPath))
				throw new ArgumentNullException(nameof(metricPath));
			_metricsCollection = metricsCollection ?? throw new ArgumentNullException(nameof(metricsCollection));
			_metricsItem = new DiagnosticContextMetricsItem(metricTypes, metricPath, diagnosticContextLogger);
			_internalMetricsItem = new DiagnosticContextInternalMetricsItem(metricTypes);
			_totalTimer = _metricsItem.DynamicSteps.StartTotal();

			_sourceCodeLabelScope = isFeatureBoundaryCodePoint
				? CodeFlowMonitoringService.SetCodeSourceLabel(PrefixName)
				: CodeFlowMonitoringService.TrySetCodeSourceLabel(PrefixName);
		});
	}

	internal DiagnosticContext(DiagnosticContextMetricsItem metricsItem)
	{
		_safeExceptionHandler = new SafeExceptionHandler();
		_safeExceptionHandler.HandleExceptions(() =>
		{
			_metricsItem = metricsItem ?? throw new ArgumentNullException(nameof(metricsItem));
			_totalTimer = metricsItem.DynamicSteps.StartTotal();
		});
	}

	public IDisposable MeasureForAdditionalMetric(IDiagnosticContext diagnosticContext)
	{
		return _safeExceptionHandler.HandleExceptions(
			() =>
			{
				_diagnosticContextCollection.LinkDiagnosticContext(diagnosticContext);
				return new AdditionalContextDisposableContainer(diagnosticContext, _diagnosticContextCollection);
			},
			() => NullDisposable.Instance);
	}

	public IMeasurement Measure(string stepName)
	{
		var measurement = _safeExceptionHandler.HandleExceptions(() =>
		{
			if (_safeExceptionHandler.IsInInvalidState)
				return new FakeTimer();

			if (string.IsNullOrEmpty(stepName))
				throw new ArgumentNullException(stepName);

			return new DisposableContainer(
					_diagnosticContextCollection.Measure(stepName),
					_metricsItem.DynamicSteps.StartStep(stepName))
				as IDisposable;
		},
		() => new FakeTimer());

		return new NullMeasurementTagsAdapter(measurement);
	}

	public void SetTag(string tag, string value)
	{
		_safeExceptionHandler.HandleExceptions(
			() =>
			{
				_metricsCollection.SetTag(tag, value);
			});
	}

	public void Increment(string counterPath)
	{
		_safeExceptionHandler.HandleExceptions(() =>
		{
			_diagnosticContextCollection.Increment(counterPath);

			if (!_metricsItem.Counters.ContainsKey(counterPath))
				_metricsItem.Counters[counterPath] = 0;
			_metricsItem.Counters[counterPath]++;
		});
	}

	public IDisposable ExtendCodeSourceLabel(string extensionCodeSourceLabel)
	{
		return new DisposableExtendCodeSourceLabel(extensionCodeSourceLabel);
	}

	public void ReportValue(string counterPath, long value)
	{
		_safeExceptionHandler.HandleExceptions(() =>
		{
			if (_metricsItem.ReportedValues.ContainsKey(counterPath))
				throw new InvalidOperationException($"Значение метрики {counterPath} было указано более одного раза.");

			_metricsItem.ReportedValues[counterPath] = value;
		});
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_safeExceptionHandler.HandleExceptions(() =>
			{
				_diagnosticContextCollection.Dispose();

				_totalTimer.Dispose();

				if (_safeExceptionHandler.IsInInvalidState)
					return;

				if (_metricsItem.DynamicSteps.IsInInvalidState)
					return;

				_internalMetricsItem.ProcessingTimeMeasurer.Measure(() => _metricsItem.PrepareForCollection());
				_internalMetricsItem.LayersCountMeasurers.Measure(_metricsItem);

				_metricsCollection?.CollectItemData(_metricsItem);

				_safeExceptionHandler.HandleExceptions(() =>
				{
					_metricsCollection?.CollectDiagnosticContextInternalMetrics(_internalMetricsItem, _metricsItem);
				});
			});

			_safeExceptionHandler.HandleExceptions(() => _sourceCodeLabelScope?.Dispose());
		}

		_disposed = true;
	}
}
