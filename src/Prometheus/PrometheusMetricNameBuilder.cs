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

namespace Mindbox.DiagnosticContext.Prometheus
{
	internal class PrometheusMetricNameBuilder
	{
		private readonly string prefix;

		private readonly string? postfix;

		private static readonly Regex InvalidCharactersRegex =
			new("[^a-zA-Z0-9_:]*", RegexOptions.Compiled);

		public PrometheusMetricNameBuilder(string prefix = "diagnosticcontext", string? postfix = null)
		{
			this.prefix = prefix;
			this.postfix = postfix;
		}

		public string BuildFullMetricName(string metricName)
		{
			var fullMetricName = string.IsNullOrEmpty(postfix)
				? $"{prefix}_{metricName}".ToLower()
				: $"{prefix}_{metricName}_{postfix}".ToLower();

			return RemoveInvalidCharactersFromMetricName(fullMetricName);
		}

		private static string RemoveInvalidCharactersFromMetricName(string metricName) =>
			InvalidCharactersRegex.Replace(metricName, "");
	}
}