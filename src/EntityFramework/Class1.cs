using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Mindbox.DiagnosticContext.EntityFramework;

public class EfExecutedCommandsMetrics
{
	private static readonly AsyncLocal<EfExecutedCommandsMetrics> _instance = new();

	public static EfExecutedCommandsMetrics Instance
	{
		get
		{
			return _instance.Value ??= new EfExecutedCommandsMetrics();
		}
	}

	public long NumOfExecutedCommands { get; private set; }

	public void ReportCommandStarted()
	{
		NumOfExecutedCommands++;
	}
}

public sealed class EfExecutedCommandsMetricsType : MetricsType<EfExecutedCommandsMeasurer>
{
	public EfExecutedCommandsMetricsType() : base(new DefaultCurrentTimeAccessor(), "SqlCommandsExecuted")
	{
	}

	public override string Units => "[commands]";

	protected override EfExecutedCommandsMeasurer CreateMeasurerCore() => new(CurrentTimeAccessor);
}

public sealed class EfExecutedCommandsMeasurer : MetricsMeasurer
{
	private long _commandsExecutedOnStop;
	private long _commandsExecutedOnStart;

	public EfExecutedCommandsMeasurer(ICurrentTimeAccessor currentTimeAccessor)
		: base(currentTimeAccessor, "EfExecutedCommands")
	{
	}

	protected override long? GetValueCore()
	{
		return _commandsExecutedOnStop - _commandsExecutedOnStart;
	}

	protected override void StartCore()
	{
		_commandsExecutedOnStart = EfExecutedCommandsMetrics.Instance.NumOfExecutedCommands;
	}

	protected override void StopCore()
	{
		_commandsExecutedOnStop = EfExecutedCommandsMetrics.Instance.NumOfExecutedCommands;
	}
}

public class EfCommandsScorerInterceptor : DbCommandInterceptor
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
		EfExecutedCommandsMetrics.Instance.ReportCommandStarted();
		return result;
	}

	private static T ReportCommandFinished<T>(T result)
	{
		// NOTE: for future purpose (example: execution time)
		return result;
	}
}