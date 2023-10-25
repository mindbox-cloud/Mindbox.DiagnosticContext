using System.Threading;

namespace Mindbox.DiagnosticContext.EntityFramework;

internal class EfCommandsMetrics
{
	private static readonly AsyncLocal<EfCommandsMetrics> _instance = new();

	public static EfCommandsMetrics Instance
	{
		get
		{
			return _instance.Value ??= new EfCommandsMetrics();
		}
	}

	public long NumOfExecutedCommands { get; private set; }

	public void ReportCommandStarted()
	{
		NumOfExecutedCommands++;
	}
}