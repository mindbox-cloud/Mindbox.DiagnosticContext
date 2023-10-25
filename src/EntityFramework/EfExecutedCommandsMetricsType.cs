namespace Mindbox.DiagnosticContext.EntityFramework;

public sealed class EfExecutedCommandsMetricsType : MetricsType<EfExecutedCommandsMeasurer>
{
	public EfExecutedCommandsMetricsType() : base(new DefaultCurrentTimeAccessor(), "SqlCommandsExecuted")
	{
	}

	public override string Units => "[commands]";

	protected override EfExecutedCommandsMeasurer CreateMeasurerCore() => new(CurrentTimeAccessor);
}