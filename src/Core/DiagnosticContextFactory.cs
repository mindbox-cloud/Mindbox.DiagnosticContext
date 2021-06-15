using System;
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext
{
	public static class DiagnosticContextFactory
	{
		private const string DiagnosticContextCreationLogicalCallContextKey = "DiagnosticContextCreationLogicalCallContextKey";

		private static IDiagnosticContext BuildCore(
			Func<IDiagnosticContext> diagnosticContextProvider, 
			IDiagnosticContextLogger diagnosticContextLogger)
		{
			try
			{
				if (IsInDiagnosticContextCreation)
					return new NullDiagnosticContext();

				IsInDiagnosticContextCreation = true;

				return diagnosticContextProvider();
			}
			catch (ObjectDisposedException)
			{
				// При получении MetricsTypesWithCpuTime у ModelApplicationHostController.Instance уже может быть вызван Dispose()
				// В этом случае так же возвращаем NullDiagnosticContext
				return new NullDiagnosticContext();
			}
			catch (Exception ex)
			{
				try
				{
					diagnosticContextLogger.Log("", ex);
				}
				catch (Exception)
				{
					// если что, вернем NullDiagnosticContext
				}

				return new NullDiagnosticContext();
			}
			finally
			{
				IsInDiagnosticContextCreation = false;
			}
		}



		public static IDiagnosticContext BuildForMetric(
			string metricPath, 
			bool isFeatureBoundaryCodePoint = false,
			MetricsType[]? metricsTypesOverride = null)
		{
			return BuildForMetric(
				metricPath,
				true,
				isFeatureBoundaryCodePoint,
				metricsTypesOverride);
		}

		public static IDiagnosticContext TryBuildForMetric(
			string metricPath, 
			bool isFeatureBoundaryCodePoint = false,
			MetricsType[]? metricsTypesOverride = null)
		{
			return BuildForMetric(
				metricPath,
				false,
				isFeatureBoundaryCodePoint,
				metricsTypesOverride);
		}

		private static IDiagnosticContext BuildForMetric(
			string metricPath,
			bool isPluginRequired,
			bool isFeatureBoundaryCodePoint,
			MetricsType[]? metricsTypesOverride)
		{
			return new NullDiagnosticContext();
		}

		public static IDiagnosticContext BuildCustom(
			Func<IDiagnosticContext> diagnosticContextProvider, 
			IDiagnosticContextLogger diagnosticContextLogger)
		{
			return BuildCore(diagnosticContextProvider, diagnosticContextLogger);
		}

		private static bool IsInDiagnosticContextCreation
		{
			get => (bool?)CallContext.LogicalGetData(DiagnosticContextCreationLogicalCallContextKey) ?? false;
			set => CallContext.LogicalSetData(DiagnosticContextCreationLogicalCallContextKey, value ? true : null);
		}

		/// <summary>
		/// Сделан для мест в которых еще недоступен ApplicationHostController и нет возможности получить плагин
		/// </summary>
		/// <param name="metricsItem">Пустая метрика транзакции</param>
		/// <returns>DiagnosticContext либо NullDiagnosticContext если произошла ошибка.</returns>
		internal static IDiagnosticContext BuildForMetricsItem(DiagnosticContextMetricsItem metricsItem)
		{
			try
			{
				return new DiagnosticContext(metricsItem);
			}
			catch (Exception)
			{
				return new NullDiagnosticContext();
			}
		}
	}
}
