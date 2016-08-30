using System.Collections.Generic;
using System.Linq;

namespace System.Diagnostics
{
	/// <summary>
	/// Logs to multiple tracers simulateously. Used for the
	/// source "inheritance"
	/// </summary>
	class AggregateTracer : ITracer
	{
		private List<DiagnosticsTracer> tracers;
		private string name;

		public AggregateTracer(string name, IEnumerable<DiagnosticsTracer> tracers)
		{
			this.name = name;
			this.tracers = tracers.ToList();
		}

		/// <summary>
		/// Traces the specified message with the given <see cref="TraceEventType"/>.
		/// </summary>
		public void Trace(TraceEventType type, object message)
		{
			tracers.ForEach(tracer => tracer.Trace(name, type, message));
		}

		/// <summary>
		/// Traces the specified formatted message with the given <see cref="TraceEventType"/>.
		/// </summary>
		public void Trace(TraceEventType type, string format, params object[] args)
		{
			tracers.ForEach(tracer => tracer.Trace(name, type, format, args));
		}

		/// <summary>
		/// Traces an exception with the specified message and <see cref="TraceEventType"/>.
		/// </summary>
		public void Trace(TraceEventType type, Exception exception, object message)
		{
			tracers.ForEach(tracer => tracer.Trace(name, type, exception, message));
		}

		/// <summary>
		/// Traces an exception with the specified formatted message and <see cref="TraceEventType"/>.
		/// </summary>
		public void Trace(TraceEventType type, Exception exception, string format, params object[] args)
		{
			tracers.ForEach(tracer => tracer.Trace(name, type, exception, format, args));
		}

		public override string ToString()
		{
			return "Aggregate sources for " + name;
		}
	}
}
