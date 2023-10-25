using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Mindbox.DiagnosticContext.EntityFramework;

internal class EfCommandsScorerInterceptor : DbCommandInterceptor
{
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

	private static T ReportCommandStarted<T>(T result)
	{
		EfCommandsMetrics.Instance.ReportCommandStarted();
		return result;
	}

	private static T ReportCommandFinished<T>(T result)
	{
		// NOTE: for future purpose (example: execution time)
		return result;
	}
}