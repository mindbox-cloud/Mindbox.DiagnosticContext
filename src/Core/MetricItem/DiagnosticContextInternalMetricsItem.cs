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

using System.Collections.Generic;
using System.Linq;
using Mindbox.DiagnosticContext.MetricsTypes;

namespace Mindbox.DiagnosticContext.MetricItem;

public class DiagnosticContextInternalMetricsItem
{
	public DiagnosticContextInternalProcessingTimeMeasurer ProcessingTimeMeasurer { get; }
	public IEnumerable<DiagnosticContextLayersCountMeasurer> LayersCountMeasurers { get; }

	public DiagnosticContextInternalMetricsItem(MetricsTypeCollection metricTypes)
	{
		ProcessingTimeMeasurer = new DiagnosticContextInternalProcessingTimeMeasurer();
		LayersCountMeasurers = metricTypes.MetricsTypes
			.Select(type => new DiagnosticContextLayersCountMeasurer(type.SystemName))
			.ToArray();
	}
}