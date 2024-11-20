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

using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Mindbox.DiagnosticContext.EntityFramework;

internal class EfCommandsScorerInterceptor : DbCommandInterceptor
{
	private readonly IEnumerable<IEfCommandMetricsCounter> _metricsCounters;

	public EfCommandsScorerInterceptor(IEnumerable<IEfCommandMetricsCounter> metricsCounters)
	{
		_metricsCounters = metricsCounters;
	}

	public override InterceptionResult<DbDataReader> ReaderExecuting(
		DbCommand command,
		CommandEventData eventData,
		InterceptionResult<DbDataReader> result) => ReportCommandStarted(result);

	public override InterceptionResult<object> ScalarExecuting(
		DbCommand command,
		CommandEventData eventData,
		InterceptionResult<object> result) => ReportCommandStarted(result);

	public override InterceptionResult<int> NonQueryExecuting(
		DbCommand command,
		CommandEventData eventData,
		InterceptionResult<int> result) => ReportCommandStarted(result);

	public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
		DbCommand command,
		CommandEventData eventData,
		InterceptionResult<DbDataReader> result,
		CancellationToken cancellationToken = default) => ReportCommandStarted(result);

	public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
		DbCommand command,
		CommandEventData eventData,
		InterceptionResult<object> result,
		CancellationToken cancellationToken = default) => ReportCommandStarted(result);

	public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
		DbCommand command,
		CommandEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default) => ReportCommandStarted(result);

	public override DbDataReader ReaderExecuted(
		DbCommand command,
		CommandExecutedEventData eventData,
		DbDataReader result) => ReportCommandFinished(result);

	public override object? ScalarExecuted(
		DbCommand command,
		CommandExecutedEventData eventData,
		object? result) => ReportCommandFinished(result);

	public override int NonQueryExecuted(
		DbCommand command,
		CommandExecutedEventData eventData,
		int result) => ReportCommandFinished(result);

	public override async ValueTask<DbDataReader> ReaderExecutedAsync(
		DbCommand command,
		CommandExecutedEventData eventData,
		DbDataReader result,
		CancellationToken cancellationToken = default) => ReportCommandFinished(result);

	public override async ValueTask<object?> ScalarExecutedAsync(
		DbCommand command,
		CommandExecutedEventData eventData,
		object? result,
		CancellationToken cancellationToken = default) => ReportCommandFinished(result);

	public override async ValueTask<int> NonQueryExecutedAsync(
		DbCommand command,
		CommandExecutedEventData eventData,
		int result,
		CancellationToken cancellationToken = default) => ReportCommandFinished(result);

	private T ReportCommandStarted<T>(T result)
	{
		foreach (var counter in _metricsCounters)
			counter.ReportCommandStarted();

		return result;
	}

	private static T ReportCommandFinished<T>(T result)
	{
		// NOTE: for future purpose (example: execution time)
		return result;
	}
}