// Copyright 2021 Mindbox Ltd
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Mindbox.DiagnosticContext.Sql;

public class AsyncLocalSqlCommandsDiagnosticMetricsCollector : ISqlCommandsDiagnosticMetricsCollector
{
	private static readonly AsyncLocal<MetricsBox> _metrics = new();

	public int SqlCommandsExecuted => _metrics.Value?.CommandsExecuted ?? 0;

	public void ReportCreatedCommand()
	{
		var metrics = _metrics.Value ??= new MetricsBox();
		metrics.CommandsExecuted++;
	}

	private class MetricsBox
	{
		public int CommandsExecuted;
	}
}