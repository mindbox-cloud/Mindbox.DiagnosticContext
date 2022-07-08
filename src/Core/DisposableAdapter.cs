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

namespace Mindbox.DiagnosticContext;

internal class DisposableAdapter<TAdaptee> : IDisposable
{
	private readonly TAdaptee _adaptee;
	private readonly Action<TAdaptee> _onDispose;

	public DisposableAdapter(TAdaptee adaptee, Action<TAdaptee> onCreation, Action<TAdaptee> onDispose)
	{
		if (onCreation == null)
			throw new ArgumentNullException(nameof(onCreation));
		_adaptee = adaptee;
		_onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
		onCreation(adaptee);
	}

	public void Dispose()
	{
		_onDispose(_adaptee);
	}
}