namespace Mindbox.DiagnosticContext.Sql;

public sealed class SqlCommandsCountMeasurer : MetricsMeasurer
{
	private long _commandsExecutedOnStart;
	private long _commandsExecutedOnStop;

	public SqlCommandsCountMeasurer(
		ICurrentTimeAccessor currentTimeAccessor,
		string metricsTypeSystemName) : base(currentTimeAccessor, metricsTypeSystemName)
	{
		SqlDiagnosticMetricsCollector.EnsureInitializedForCurrentCallContext();
	}

	protected override long? GetValueCore() => _commandsExecutedOnStop - _commandsExecutedOnStart;
	protected override void StartCore() => _commandsExecutedOnStart = SqlDiagnosticMetricsCollector.TotalCommandsExecutedInCurrentCallContext;
	protected override void StopCore() => _commandsExecutedOnStop = SqlDiagnosticMetricsCollector.TotalCommandsExecutedInCurrentCallContext;
}