namespace Mindbox.DiagnosticContext.Sql;

public sealed class SqlCommandsCountMeasurer : MetricsMeasurer
{
	private readonly ISqlCommandsDiagnosticMetricsCollector _metricsCollector;

	private long _commandsExecutedOnStop;
	private long _commandsExecutedOnStart;

	public SqlCommandsCountMeasurer(
		ISqlCommandsDiagnosticMetricsCollector metricsCollector,
		ICurrentTimeAccessor currentTimeAccessor,
		string metricsTypeSystemName) : base(currentTimeAccessor, metricsTypeSystemName)
	{
		_metricsCollector = metricsCollector;
	}

	protected override long? GetValueCore() => _commandsExecutedOnStop - _commandsExecutedOnStart;
	protected override void StartCore() => _commandsExecutedOnStart = _metricsCollector.SqlCommandsExecuted;
	protected override void StopCore() => _commandsExecutedOnStop = _metricsCollector.SqlCommandsExecuted;
}