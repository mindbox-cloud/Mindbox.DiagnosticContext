using System;
using System.Threading.Tasks;

namespace Mindbox.DiagnosticContext
{
	public interface IDiagnosticContextLogger
	{
		void Log(string message, Exception exception);
	}

	internal class NullDiagnosticContextLogger :IDiagnosticContextLogger
	{
		public void Log(string message, Exception exception)
		{
			
		}
	}
	
	internal class SafeExceptionHandler
	{
		private readonly IDiagnosticContextLogger diagnosticContextLogger;

		public SafeExceptionHandler(IDiagnosticContextLogger diagnosticContextLogger)
		{
			this.diagnosticContextLogger = diagnosticContextLogger;
		}

		public SafeExceptionHandler() : this(new NullDiagnosticContextLogger())
		{
		}

		public static SafeExceptionHandler Default { get; } = new();
		
		public bool IsInInvalidState { get; private set; }

		public bool HandleExceptions(Action action, string? errorMessage = null)
		{
			return HandleExceptions(
				() =>
				{
					action();
					return true;
				},
				() => false,
				() => errorMessage
			);
		}
		
		public Task<bool> HandleExceptionsAsync(Func<Task> action, string? errorMessage = null)
		{
			return HandleExceptionsAsync(
				async () =>
				{
					await action().ConfigureAwait(false);
					return true;
				},
				() => false,
				() => errorMessage
			);
		}
		
		public async Task<TResult> HandleExceptionsAsync<TResult>(
			Func<Task<TResult>> action,
			Func<TResult> invalidResultBuilderAction,
			Func<string?>? errorMessageBuilder = null)
		{
			try
			{
				return await action().ConfigureAwait(false);
			}
			catch (Exception innerException)
			{
				return ProcessException(invalidResultBuilderAction, errorMessageBuilder, innerException);
			}
		}

		public TResult HandleExceptions<TResult>(
			Func<TResult> action, 
			Func<TResult> invalidResultBuilderAction,
			Func<string?>? errorMessageBuilder = null)
		{
			try
			{
				return action();
			}
			catch (Exception innerException)
			{
				return ProcessException(invalidResultBuilderAction, errorMessageBuilder, innerException);
			}
		}

		private TResult ProcessException<TResult>(
			Func<TResult> invalidResultBuilderAction,
			Func<string?>? errorMessageBuilder,
			Exception innerException)
		{
			IsInInvalidState = true;

			try
			{
				var errorMessage = errorMessageBuilder?.Invoke() ?? innerException.Message;

				diagnosticContextLogger
					.Log(errorMessage, innerException);
			}
			catch (Exception)
			{
				// Весь смысл SafeExceptionHandler - не допустить бросание исключений из action
			}

			return invalidResultBuilderAction();
		}
	}
}
