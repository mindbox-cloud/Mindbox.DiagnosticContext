using System;

namespace Mindbox.DiagnosticContext
{
	public interface ICurrentTimeAccessor
	{
		DateTime CurrentDateTimeUtc { get; }
	}
}