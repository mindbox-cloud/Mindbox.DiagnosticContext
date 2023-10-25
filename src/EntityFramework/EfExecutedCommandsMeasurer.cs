namespace Mindbox.DiagnosticContext.EntityFramework;

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
		_commandsExecutedOnStart = EfCommandsMetrics.Instance.NumOfExecutedCommands;
	}

	protected override void StopCore()
	{
		_commandsExecutedOnStop = EfCommandsMetrics.Instance.NumOfExecutedCommands;
	}
}