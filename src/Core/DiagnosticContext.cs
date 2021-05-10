#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itc.Commons.Model
{
	public class DiagnosticContext : IDiagnosticContext
	{
		private readonly DiagnosticContextCollection diagnosticContextCollection = new DiagnosticContextCollection();
		private IDiagnosticContextMetricsCollection metricsCollection;
		private DiagnosticContextMetricsItem metricsItem;
		private IDisposable totalTimer;

		private bool disposed;
		private readonly SafeExceptionHandler safeExceptionHandler = new SafeExceptionHandler(ItcLogLevel.Error);

		public string PrefixName => metricsItem?.MetricPrefix;

		private IDisposable sourceCodeLabelScope = null;

		public DiagnosticContext(
			string metricPath, 
			IDiagnosticContextMetricsCollection metricsCollection,
			MetricsTypeCollection metricTypes,
			bool isFeatureBoundaryCodePoint = false)
		{
			safeExceptionHandler.HandleExceptions(() =>
			{
				if (string.IsNullOrEmpty(metricPath))
					throw new ArgumentNullException(nameof(metricPath));

				if (metricsCollection == null)
					throw new ArgumentNullException(nameof(metricsCollection));
				
				this.metricsCollection = metricsCollection;
				metricsItem = new DiagnosticContextMetricsItem(metricTypes, metricPath);
				totalTimer = metricsItem.DynamicSteps.StartTotal();

				sourceCodeLabelScope = isFeatureBoundaryCodePoint 
					? CodeFlowMonitoringService.SetCodeSourceLabel(PrefixName)
					: CodeFlowMonitoringService.TrySetCodeSourceLabel(PrefixName);
			});
		}

		internal DiagnosticContext(DiagnosticContextMetricsItem metricsItem)
		{
			safeExceptionHandler.HandleExceptions(() =>
			{
				if (metricsItem == null)
					throw new ArgumentNullException(nameof(metricsItem));

				this.metricsItem = metricsItem;
				totalTimer = metricsItem.DynamicSteps.StartTotal();
			});
		}
		
		public IDisposable MeasureForAdditionalMetric(IDiagnosticContext diagnosticContext)
		{
			return safeExceptionHandler.HandleExceptions(
				() =>
				{
					diagnosticContextCollection.LinkDiagnosticContext(diagnosticContext);
					return diagnosticContext;
				},
				() => NullDisposable.Instance);
		}

		public IDisposable Measure(string stepName)
		{
			return safeExceptionHandler.HandleExceptions(() =>
			{
				if (safeExceptionHandler.IsInInvalidState)
					return new FakeTimer();

				if (string.IsNullOrEmpty(stepName))
					throw new ArgumentNullException(stepName);

				return new DisposableContainer(
						diagnosticContextCollection.Measure(stepName),
						metricsItem.DynamicSteps.StartStep(stepName))
					as IDisposable;
			},
			() => new FakeTimer());
		}

		public void SetTag(string tag, string value)
		{
			safeExceptionHandler.HandleExceptions(
				() =>
				{
					metricsCollection.SetTag(tag, value);
				});
		}

		public void Increment(string counterPath)
		{
			safeExceptionHandler.HandleExceptions(() =>
			{
				diagnosticContextCollection.Increment(counterPath);

				if (!metricsItem.Counters.ContainsKey(counterPath))
					metricsItem.Counters[counterPath] = 0;
				metricsItem.Counters[counterPath]++;
			});
		}

		public IDisposable ExtendCodeSourceLabel(string extensionCodeSourceLabel)
		{
			return new DisposableExtendCodeSourceLabel(extensionCodeSourceLabel);
		}

		public void ReportValue(string counterPath, long value)
		{
			safeExceptionHandler.HandleExceptions(() =>
			{
				if (metricsItem.ReportedValues.ContainsKey(counterPath))
					throw new InvalidOperationException($"Значение метрики {counterPath} было указано более одного раза.");

				metricsItem.ReportedValues[counterPath] = value;
			});
		}

		public void Dispose()
		{
			if (!disposed)
			{
				safeExceptionHandler.HandleExceptions(() =>
				{
					diagnosticContextCollection.Dispose();

					totalTimer.Dispose();

					if (safeExceptionHandler.IsInInvalidState)
						return;

					if (metricsItem.DynamicSteps.IsInInvalidState)
						return;

					metricsItem.PrepareForCollection();

					metricsCollection?.CollectItemData(metricsItem);
				});

				safeExceptionHandler.HandleExceptions(() => sourceCodeLabelScope?.Dispose());
			}

			disposed = true;
		}
	}
}
