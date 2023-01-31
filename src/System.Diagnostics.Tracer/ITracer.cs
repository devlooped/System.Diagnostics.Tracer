namespace System.Diagnostics
{
	/// <summary>
	/// Interface used by the application components to log messages.
	/// </summary>
	public interface ITracer : IFluentInterface
	{
		/// <summary>
		/// Traces the specified message with the given <see cref="TraceEventType"/>.
		/// </summary>
		void Trace(TraceEventType type, object message);

		/// <summary>
		/// Traces the specified formatted message with the given <see cref="TraceEventType"/>.
		/// </summary>
		void Trace(TraceEventType type, string format, params object[] args);

		/// <summary>
		/// Traces an exception with the specified message and <see cref="TraceEventType"/>.
		/// </summary>
		void Trace(TraceEventType type, Exception exception, object message);

		/// <summary>
		/// Traces an exception with the specified formatted message and <see cref="TraceEventType"/>.
		/// </summary>
		void Trace(TraceEventType type, Exception exception, string format, params object[] args);
	}
}
