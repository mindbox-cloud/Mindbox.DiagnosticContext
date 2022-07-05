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

public static class CodeFlowMonitoringService
{
	private const string CodeSourceLabelKey = "CodeSourceLabelKey";

	public static string? TryGetCodeSourceLabel()
	{
		return (string?)CallContext.LogicalGetData(CodeSourceLabelKey);
	}

	public static IDisposable SetCodeSourceLabel(string codeSourceLabel)
	{
		CallContext.LogicalSetData(CodeSourceLabelKey, codeSourceLabel);

		return new Cleaner(CodeSourceLabelKey);
	}

	public static void Clear()
	{
		CallContext.FreeNamedDataSlot(CodeSourceLabelKey);
	}

	public static IDisposable TrySetCodeSourceLabel(string codeSourceLabel)
	{
		var currentCodeSourceLabel = TryGetCodeSourceLabel();
		if (currentCodeSourceLabel != null)
			return NullDisposable.Instance;

		return SetCodeSourceLabel(codeSourceLabel);
	}

	private class Cleaner : IDisposable
	{
		private readonly string _name;
		private bool _isDisposed;

		public Cleaner(string name)
		{
			_name = name;
		}

		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}

			CallContext.FreeNamedDataSlot(_name);
			_isDisposed = true;
		}
	}
}