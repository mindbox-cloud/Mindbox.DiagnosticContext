namespace Mindbox.DiagnosticContext.Sql;

public class AsyncLocalSqlCommandsDiagnosticMetricsCollector : ISqlCommandsDiagnosticMetricsCollector
{
	private static readonly AsyncLocal<MetricsBox> _metrics = new();

	public int SqlCommandsExecuted => _metrics.Value?.CommandsExecuted ?? 0;

	public void ReportCreatedCommand()
	{
		var metrics = _metrics.Value ??= new MetricsBox();
		metrics.CommandsExecuted++;
	}

	private class MetricsBox
	{
		public int CommandsExecuted;
	}
}