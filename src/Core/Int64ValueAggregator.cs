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

using System;

namespace Mindbox.DiagnosticContext;

public class Int64ValueAggregator
{
	private long? _min;

	public long Count { get; private set; }
	public long Total { get; private set; }
	public long Max { get; private set; }

	public long Min => _min ?? 0;

	public void Add(long value)
	{
		if (value < 0)
			throw new ArgumentOutOfRangeException(nameof(value), "value < 0");

		Total += value;
		if ((_min == null) || (value < _min))
			_min = value;
		if (value > Max)
			Max = value;

		Count++;
	}

	public Int64MetricData ToMetricData(long? countOverride = null)
	{
		return new()
		{
			Total = Total,
			Count = countOverride ?? Count,
			Max = Max,
			Min = Min
		};
	}
}

public class Int64MetricData
{
	public long Total { get; set; }
	public long? Count { get; set; }
	public long? Max { get; set; }
	public long? Min { get; set; }
}