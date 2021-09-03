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
using System.Runtime.InteropServices;

namespace Mindbox.DiagnosticContext.CpuTimeByFeatureMetrics
{
	internal sealed class DefaultProcessTotalProcessorTimeProvider : IProcessTotalProcessorTimeProvider
	{
		private static readonly IntPtr currentProcessPseudoHandle = new IntPtr(-1);

		[DllImport("kernel32.dll")]
		public static extern bool GetProcessTimes(
			IntPtr handle,
			out long creationTime,
			out long exitTime,
			out long kernelTime,
			out long userTime);

		public TimeSpan GetCurrentProcessorTime()
		{
			GetProcessTimes(currentProcessPseudoHandle, out var _, out var _, out var kernelTime, out var userTime);

			return TimeSpan.FromTicks(kernelTime + userTime);
		}
	}
}
