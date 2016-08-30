using System.ComponentModel;

namespace System.Diagnostics
{
	/// <summary>
	/// Extensions to <see cref="ITracer"/> for activity tracing.
	/// </summary>
	/// <remarks>
	/// Under netstandard, there is no activity tracing since all the TraceEventType 
	/// values are missing. API provided for compatibility.
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class StartActivityExtension
	{
		static readonly IDisposable disposable = new NullDisposable();

		/// <summary>
		/// Starts a new activity scope.
		/// </summary>
		public static IDisposable StartActivity(this ITracer tracer, string format, params object[] args)
		{
			return disposable;
		}

		/// <summary>
		/// Starts a new activity scope.
		/// </summary>
		public static IDisposable StartActivity(this ITracer tracer, string displayName)
		{
			return disposable;
		}

		class NullDisposable : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}
}
