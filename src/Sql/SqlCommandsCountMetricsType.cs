namespace Mindbox.DiagnosticContext.Sql;

public class SqlCommandsCountMetricsType : MetricsType<SqlCommandsCountMeasurer>
{
	public SqlCommandsCountMetricsType() : base(new DefaultCurrentTimeAccessor(), "SqlCommandsExecuted")
	{
	}

	public override string Units => "[commands]";

	protected override SqlCommandsCountMeasurer CreateMeasurerCore() => new(CurrentTimeAccessor, SystemName);
}