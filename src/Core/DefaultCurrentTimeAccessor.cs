using System;

namespace Mindbox.DiagnosticContext
{
    public class DefaultCurrentTimeAccessor : ICurrentTimeAccessor
    {
        public DateTime CurrentDateTimeUtc => DateTime.UtcNow;
    }
}