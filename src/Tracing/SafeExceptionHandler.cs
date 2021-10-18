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

namespace Mindbox.DiagnosticContext.Tracing
{
	internal class SafeExceptionHandler
	{
		private readonly Action<Exception> exceptionHandler;

		public SafeExceptionHandler(Action<Exception> exceptionHandler)
		{
			this.exceptionHandler = exceptionHandler;
		}
		
		public void Execute(Action action)
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				HandleException(exception);
			}
		}
		
		public TValue Execute<TValue>(Func<TValue> action, Func<TValue> fallback)
		{
			try
			{
				return action();
			}
			catch (Exception exception)
			{
				HandleException(exception);
				return fallback();
			}
		}
		
		private void HandleException(Exception exception)
		{
			try
			{
				exceptionHandler(exception);
			}
			catch { /* do nothing */ }
		}
	}
}