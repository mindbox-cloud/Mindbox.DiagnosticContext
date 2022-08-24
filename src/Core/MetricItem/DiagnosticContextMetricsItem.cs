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

#nullable disable

using System;
using System.Collections.Generic;
using Mindbox.DiagnosticContext.DynamicSteps;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext.MetricItem;

public class DiagnosticContextMetricsItem
{
	private DiagnosticContextMetricsNormalizedValueCollection _normalizedMetricsValues;

	public DiagnosticContextMetricsItem(
		MetricsTypeCollection metricsTypes,
		string metricPrefix,
		IDiagnosticContextLogger diagnosticContextLogger)
	{
		MetricsTypes = metricsTypes;
		MetricPrefix = metricPrefix;

		DynamicSteps = new DiagnosticContextDynamicSteps(metricsTypes, diagnosticContextLogger);
	}

	internal void PrepareForCollection()
	{
		_normalizedMetricsValues = DynamicSteps.GetNormalizedMetricsValues();
	}

	public DiagnosticContextMetricsNormalizedValueCollection GetNormalizedMetricsValues()
	{
		if (_normalizedMetricsValues == null)
			throw new InvalidOperationException($"Metrics hasn't been collected yet");

		return _normalizedMetricsValues;
	}

	public MetricsTypeCollection MetricsTypes { get; }
	public string MetricPrefix { get; }
	public bool IsEmpty => false;

	public DiagnosticContextDynamicSteps DynamicSteps { get; }

	public Dictionary<string, int> Counters { get; } = new Dictionary<string, int>();
	public Dictionary<string, long> ReportedValues { get; } = new Dictionary<string, long>();
}