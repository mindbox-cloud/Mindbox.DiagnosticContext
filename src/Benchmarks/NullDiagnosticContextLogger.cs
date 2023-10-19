using Microsoft.Extensions.Logging;
using Mindbox.DiagnosticContext;

public class NullDiagnosticContextLogger : IDiagnosticContextLogger
{
    public void Log(string message, Exception? exception = null, LogLevel? logLevel = null, IDictionary<string, object>? additionalProperties = null)
    {
    }
}