namespace Mindbox.DiagnosticContext.EntityFramework;

public interface IEfExecutedCommandsMetricProvider
{
	long NumOfExecutedCommands { get; }
}

public sealed class EfExecutedCommandsMetricsType : MetricsType<EfExecutedCommandsMeasurer>
{
	private readonly IEfExecutedCommandsMetricProvider _executedCommandsMetricProvider;

	public EfExecutedCommandsMetricsType(IEfExecutedCommandsMetricProvider executedCommandsMetricProvider)
		: base(new DefaultCurrentTimeAccessor(), "SqlCommandsExecuted")
	{
		_executedCommandsMetricProvider = executedCommandsMetricProvider;
	}

	public override string Units => "[commands]";

	protected override EfExecutedCommandsMeasurer CreateMeasurerCore() => new(CurrentTimeAccessor, _executedCommandsMetricProvider);
}

public sealed class EfExecutedCommandsMeasurer : MetricsMeasurer
{
	private readonly IEfExecutedCommandsMetricProvider _executedCommandsMetricProvider;

	private long _commandsExecutedOnStop;
	private long _commandsExecutedOnStart;

	public EfExecutedCommandsMeasurer(ICurrentTimeAccessor currentTimeAccessor, IEfExecutedCommandsMetricProvider executedCommandsMetricProvider)
		: base(currentTimeAccessor, "EfExecutedCommands")
	{
		_executedCommandsMetricProvider = executedCommandsMetricProvider;
	}

	protected override long? GetValueCore()
	{
		return _commandsExecutedOnStop - _commandsExecutedOnStart;
	}

	protected override void StartCore()
	{
		_commandsExecutedOnStart = _executedCommandsMetricProvider.NumOfExecutedCommands;
	}

	protected override void StopCore()
	{
		_commandsExecutedOnStop = _executedCommandsMetricProvider.NumOfExecutedCommands;
	}
}