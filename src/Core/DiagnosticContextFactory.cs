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
using Mindbox.DiagnosticContext.MetricItem;

namespace Mindbox.DiagnosticContext;

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
#pragma warning disable IDE0060 // Remove unused parameter
		string metricPath,
		bool isPluginRequired,
		bool isFeatureBoundaryCodePoint,
		MetricsType[]? metricsTypesOverride)
#pragma warning restore IDE0060 // Remove unused parameter
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