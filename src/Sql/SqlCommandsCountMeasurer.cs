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

public sealed class SqlCommandsCountMeasurer : MetricsMeasurer
{
	private readonly ISqlCommandsDiagnosticMetricsCollector _metricsCollector;

	private long _commandsExecutedOnStop;
	private long _commandsExecutedOnStart;

	public SqlCommandsCountMeasurer(
		ISqlCommandsDiagnosticMetricsCollector metricsCollector,
		ICurrentTimeAccessor currentTimeAccessor,
		string metricsTypeSystemName) : base(currentTimeAccessor, metricsTypeSystemName)
	{
		_metricsCollector = metricsCollector;
	}

	protected override long? GetValueCore() => _commandsExecutedOnStop - _commandsExecutedOnStart;
	protected override void StartCore() => _commandsExecutedOnStart = _metricsCollector.SqlCommandsExecuted;
	protected override void StopCore() => _commandsExecutedOnStop = _metricsCollector.SqlCommandsExecuted;
}