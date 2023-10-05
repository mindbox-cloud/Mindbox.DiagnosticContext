namespace Mindbox.DiagnosticContext.Sql;

public static class SqlDiagnosticMetricsCollector
{
	private static readonly AsyncLocal<MetricsStorage?> _contextMetricsStorage = new();

	internal static int TotalCommandsExecutedInCurrentCallContext => _contextMetricsStorage.Value?.CommandsExecuted ?? 0;

	public static void ReportSqlCommandExecuting()
	{
		if (_contextMetricsStorage.Value is { } storage)
			storage.CommandsExecuted++;
	}

	internal static void EnsureInitializedForCurrentCallContext() => _contextMetricsStorage.Value ??= new MetricsStorage();

	private class MetricsStorage
	{
		public int CommandsExecuted { get; set; }
	}
}