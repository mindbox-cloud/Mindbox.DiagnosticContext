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

namespace Mindbox.DiagnosticContext
{
	internal abstract class MetricsMeasurer
	{
		private const long InitialValue = 0;

		private bool isStarted = false;
		private bool isStopped = false;

		protected bool IsRoot { get; private set; }

		internal MetricsMeasurer(ICurrentTimeAccessor currentTimeAccessor, string metricsTypeSystemName)
		{
			if (string.IsNullOrEmpty(metricsTypeSystemName))
				throw new ArgumentNullException(nameof(metricsTypeSystemName));

			CurrentTimeAccessor = currentTimeAccessor;
			MetricsTypeSystemName = metricsTypeSystemName;
		}

		protected ICurrentTimeAccessor CurrentTimeAccessor { get; }
		public string MetricsTypeSystemName { get; }
		public bool IsStarted => isStarted;
		public bool IsStopped => isStopped;
		
		public void Start()
		{
			if (isStarted)
				throw new InvalidOperationException("You can't start one measurer twice");

			StartCore();

			isStarted = true;
		}

		public void Stop()
		{
			if (!isStarted)
				throw new InvalidOperationException("You should start measurer before stopping it");
			if (isStopped)
				throw new InvalidOperationException("You can't stop one measurer twice");

			isStopped = true;
			StopCore();
		}

		public long? GetValue()
		{
			if (!isStarted)
				return InitialValue;

			return GetValueCore();
		}

		public void MarkAsRoot()
		{
			if (IsStarted)
				throw new InvalidOperationException("Can't mark metrics as root, it's already started");

			IsRoot = true;
		}

		protected abstract long? GetValueCore();
		protected abstract void StartCore();
		protected abstract void StopCore();
	}
}
