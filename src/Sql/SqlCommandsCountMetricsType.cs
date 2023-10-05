namespace Mindbox.DiagnosticContext.Sql;

public class SqlCommandsCountMetricsType : MetricsType<SqlCommandsCountMeasurer>
{
	private readonly ISqlCommandsDiagnosticMetricsCollector _metricsCollector;

	public SqlCommandsCountMetricsType(ISqlCommandsDiagnosticMetricsCollector metricsCollector)
		: base(new DefaultCurrentTimeAccessor(), "SqlCommandsExecuted")
	{
		_metricsCollector = metricsCollector;
	}

	public override string Units => "[commands]";

	protected override SqlCommandsCountMeasurer CreateMeasurerCore() => new(_metricsCollector, CurrentTimeAccessor, SystemName);
}