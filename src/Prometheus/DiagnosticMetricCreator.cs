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
using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus;

internal sealed class DiagnosticMetricCreator
{
	private readonly Func<string, string, string[], ICounterAdapter> _createCounter;

	public DiagnosticMetricCreator(IMetricFactory factory)
	{
		_createCounter = (name, help, labelNames) =>
			new PlainCounterAdapter(factory.CreateCounter(name, help,
				new CounterConfiguration { LabelNames = labelNames }));
	}

	public DiagnosticMetricCreator(IManagedLifetimeMetricFactory factory)
	{
		_createCounter = (name, help, labelNames) =>
			new ManagedLifetimeCounterAdapter(
				factory.CreateCounter(name, help, labelNames).WithExtendLifetimeOnUse());
	}

	public ICounterAdapter CreateCounter(string name, string help, string[] labelNames) =>
		_createCounter(name, help, labelNames);
}

internal interface ICounterAdapter
{
	void Inc(string[] labelValues, double amount);
}

internal sealed class PlainCounterAdapter : ICounterAdapter
{
	private readonly Counter _counter;

	public PlainCounterAdapter(Counter counter) => _counter = counter;

	public void Inc(string[] labelValues, double amount) =>
		_counter.WithLabels(labelValues).Inc(amount);
}

internal sealed class ManagedLifetimeCounterAdapter : ICounterAdapter
{
	private readonly ICollector<ICounter> _collector;

	public ManagedLifetimeCounterAdapter(ICollector<ICounter> collector) => _collector = collector;

	public void Inc(string[] labelValues, double amount) =>
		_collector.WithLabels(labelValues).Inc(amount);
}
