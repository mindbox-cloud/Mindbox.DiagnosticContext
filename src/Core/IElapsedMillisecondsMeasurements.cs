namespace Mindbox.DiagnosticContext
{
	public interface IElapsedMillisecondsMeasurements
	{
		long ElapsedMilliseconds { get; }
		long ElapsedCpuMilliseconds { get; }
	}
}
