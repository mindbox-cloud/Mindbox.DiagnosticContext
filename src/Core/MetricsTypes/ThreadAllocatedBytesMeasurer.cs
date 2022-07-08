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
using System.Linq.Expressions;

namespace Mindbox.DiagnosticContext.MetricsTypes;

internal sealed class ThreadAllocatedBytesMeasurer : MetricsMeasurer
{
	private static readonly Func<long> _getAllocatedBytesForCurrentThread;

	private long _bytesAllocatedByThreadAtStart;

	static ThreadAllocatedBytesMeasurer()
	{
		var getAllocatedBytesForCurrentThreadMethod = typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread");
		if (getAllocatedBytesForCurrentThreadMethod == null)
		{
			_getAllocatedBytesForCurrentThread = () => 0;
		}
		else
		{
			_getAllocatedBytesForCurrentThread =
				Expression.Lambda<Func<long>>(Expression.Call(getAllocatedBytesForCurrentThreadMethod))
					.Compile();
		}
	}

	public ThreadAllocatedBytesMeasurer(ICurrentTimeAccessor currentTimeAccessor, string metricsTypeSystemName)
		: base(currentTimeAccessor, metricsTypeSystemName)
	{
	}

	protected override long? GetValueCore()
	{
		return _bytesAllocatedByThreadAtStart;
	}

	protected override void StartCore()
	{
		_bytesAllocatedByThreadAtStart += _getAllocatedBytesForCurrentThread();
	}

	protected override void StopCore()
	{
		_bytesAllocatedByThreadAtStart = _getAllocatedBytesForCurrentThread() - _bytesAllocatedByThreadAtStart;
	}
}