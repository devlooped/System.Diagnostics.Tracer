using System.ComponentModel;
using System.Globalization;

namespace System.Diagnostics
{
	/// <summary>
	/// Extensions to <see cref="ITracer"/> for activity tracing.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class StartActivityExtension
	{
		/// <summary>
		/// Starts a new activity scope.
		/// </summary>
		public static IDisposable StartActivity(this ITracer tracer, string format, params object[] args)
		{
			return new TraceActivity(tracer, format, false, args);
		}

		/// <summary>
		/// Starts a new activity scope.
		/// </summary>
		public static IDisposable StartActivity(this ITracer tracer, string displayName)
		{
			return new TraceActivity(tracer, displayName);
		}

		/// <summary>
		/// Starts a new activity scope.
		/// </summary>
		public static IDisposable StartActivity(this ITracer tracer, string displayName, bool appendElapsedTime)
		{
			return new TraceActivity(tracer, displayName, appendElapsedTime);
		}

		/// <devdoc>
		/// In order for activity tracing to happen, the trace source needs to
		/// have <see cref="SourceLevels.ActivityTracing"/> enabled.
		/// </devdoc>
		class TraceActivity : IDisposable
		{
			string displayName;
			bool disposed;
			ITracer tracer;
			Guid oldId;
			Guid newId;
			bool appendElapsedTime;
			DateTime startTime;

			public TraceActivity(ITracer tracer, string displayName, bool appendElapsedTime = false)
				: this(tracer, displayName, appendElapsedTime, null)
			{ }

			public TraceActivity(ITracer tracer, string displayName, bool appendElapsedTime, params object[] args)
			{
				this.tracer = tracer;
				this.appendElapsedTime = appendElapsedTime;

				this.displayName = displayName;
				if (args != null && args.Length > 0)
					this.displayName = string.Format(displayName, args, CultureInfo.CurrentCulture);

				newId = Guid.NewGuid();
				oldId = Trace.CorrelationManager.ActivityId;

				if (oldId != Guid.Empty)
					tracer.Trace(TraceEventType.Transfer, newId);

				Trace.CorrelationManager.ActivityId = newId;

				if (appendElapsedTime)
					startTime = DateTime.UtcNow;

				tracer.Trace(TraceEventType.Start, this.displayName);
			}

			public void Dispose()
			{
				if (!disposed)
				{
					var message = displayName;
					if (appendElapsedTime)
						message += string.Format(" ({0} ms)", (int)DateTime.UtcNow.Subtract(startTime).TotalMilliseconds);

					tracer.Trace(TraceEventType.Stop, message);
					if (oldId != Guid.Empty)
						tracer.Trace(TraceEventType.Transfer, oldId);

					Trace.CorrelationManager.ActivityId = oldId;
				}

				disposed = true;
			}
		}
	}
}
