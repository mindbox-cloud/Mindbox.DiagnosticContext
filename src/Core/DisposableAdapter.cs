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

namespace Mindbox.DiagnosticContext
{
	internal class DisposableAdapter<TAdaptee> : IDisposable
	{
		private readonly TAdaptee adaptee;
		private readonly Action<TAdaptee> onDispose;

		public DisposableAdapter(TAdaptee adaptee, Action<TAdaptee> onCreation, Action<TAdaptee> onDispose)
		{
			if (onCreation == null)
				throw new ArgumentNullException(nameof(onCreation));
			if (onDispose == null)
				throw new ArgumentNullException(nameof(onDispose));

			this.adaptee = adaptee;
			this.onDispose = onDispose;
			onCreation(adaptee);
		}

		public void Dispose()
		{
			onDispose(adaptee);
		}
	}
}
