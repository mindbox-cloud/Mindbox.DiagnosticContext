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
	internal class DisposableContainer : IDisposable
	{
		private readonly IDisposable[] items;

		private bool disposed;

		public DisposableContainer(params IDisposable[] items)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			this.items = items;
		}

		public void Dispose()
		{
			if (!disposed)
			{
				foreach (var item in items)
					item?.Dispose();

				disposed = true;
			}
		}
	}
}
