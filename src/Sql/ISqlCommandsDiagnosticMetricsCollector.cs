namespace Mindbox.DiagnosticContext.Sql;

public interface ISqlCommandsDiagnosticMetricsCollector
{
	public void ReportCreatedCommand();
	public int SqlCommandsExecuted { get; }
}