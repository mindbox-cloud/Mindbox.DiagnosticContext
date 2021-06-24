﻿using System;

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