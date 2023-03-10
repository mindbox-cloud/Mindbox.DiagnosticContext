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

using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Mindbox.DiagnosticContext.Prometheus;

internal class PrometheusMetricNameBuilder
{
	private readonly PrometheusMetricNameBuilderOptions _prometheusMetricNameBuilderOptions;

	private static readonly Regex _invalidCharactersRegex =
		new("[^a-zA-Z0-9_:]+", RegexOptions.Compiled);

	public PrometheusMetricNameBuilder(IOptions<PrometheusMetricNameBuilderOptions> prometheusMetricNameBuilderOptions)
	{
		_prometheusMetricNameBuilderOptions = prometheusMetricNameBuilderOptions.Value;
	}

	public string BuildFullMetricName(string metricName)
	{
		var microServicePrefix = string.IsNullOrEmpty(_prometheusMetricNameBuilderOptions.MicroServicePrefix)
			? "" : $"_{_prometheusMetricNameBuilderOptions.MicroServicePrefix}";
		var postfix = string.IsNullOrEmpty(_prometheusMetricNameBuilderOptions.Postfix)
			? "" : $"_{_prometheusMetricNameBuilderOptions.Postfix}";

		var fullMetricName =
			$"{_prometheusMetricNameBuilderOptions.Prefix}{microServicePrefix}_{metricName}{postfix}".ToLower();

		return RemoveInvalidCharactersFromMetricName(fullMetricName);
	}

	private static string RemoveInvalidCharactersFromMetricName(string metricName) =>
		_invalidCharactersRegex.Replace(metricName, "");
}