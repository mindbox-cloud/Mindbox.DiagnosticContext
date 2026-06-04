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

using Prometheus;

namespace Mindbox.DiagnosticContext.Prometheus;

internal sealed class DiagnosticMetricCreator
{
	private readonly IMetricFactory? _plainFactory;
	private readonly IManagedLifetimeMetricFactory? _managedFactory;

	public DiagnosticMetricCreator(IMetricFactory factory)
	{
		_plainFactory = factory;
	}

	public DiagnosticMetricCreator(IManagedLifetimeMetricFactory factory)
	{
		_managedFactory = factory;
	}

	public ICounterAdapter CreateCounter(string name, string help, string[] labelNames)
	{
		if (_managedFactory != null)
		{
			var handle = _managedFactory.CreateCounter(name, help, labelNames);
			return new ManagedLabeledMetric(handle.WithExtendLifetimeOnUse());
		}

		var counter = _plainFactory!.CreateCounter(
			name,
			help,
			new CounterConfiguration { LabelNames = labelNames });
		return new PlainLabeledMetric(counter);
	}
}

internal interface ICounterAdapter
{
	void Inc(string[] labelValues, double amount);
}

internal sealed class PlainLabeledMetric : ICounterAdapter
{
	private readonly Counter _counter;

	public PlainLabeledMetric(Counter counter) => _counter = counter;

	public void Inc(string[] labelValues, double amount) =>
		_counter.WithLabels(labelValues).Inc(amount);
}

internal sealed class ManagedLabeledMetric : ICounterAdapter
{
	private readonly ICollector<ICounter> _collector;

	public ManagedLabeledMetric(ICollector<ICounter> collector) => _collector = collector;

	public void Inc(string[] labelValues, double amount) =>
		_collector.WithLabels(labelValues).Inc(amount);
}
