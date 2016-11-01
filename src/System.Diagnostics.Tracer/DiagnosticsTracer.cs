using System.Globalization;

namespace System.Diagnostics
{
	/// <summary>
	/// Implements the <see cref="ITracer"/> interface on top of
	/// <see cref="TraceSource"/>.
	/// </summary>
	class DiagnosticsTracer
	{
		TraceSource source;

		public DiagnosticsTracer(TraceSource source)
		{
			this.source = source;
		}

		public void Trace(string sourceName, TraceEventType type, object message)
		{
			lock (source)
			{
				using (new SourceNameReplacer(source, sourceName))
				{
					// Portable version has no concept of a trace transfer.
					source.TraceEvent(type, 0, message.ToString());
				}
			}
		}

		public void Trace(string sourceName, TraceEventType type, string format, params object[] args)
		{
			lock (source)
			{
				using (new SourceNameReplacer(source, sourceName))
				{
					if (args != null && args.Length > 0)
						source.TraceEvent(type, 0, format, args);
					else
						source.TraceEvent(type, 0, format);
				}
			}
		}

		public void Trace(string sourceName, TraceEventType type, Exception exception, object message)
		{
			Trace(sourceName, type, exception, message.ToString());
		}

		public void Trace(string sourceName, TraceEventType type, Exception exception, string format, params object[] args)
		{
			lock (source)
			{
				using (new SourceNameReplacer(source, sourceName))
				{
					var message = format;
					if (args != null && args.Length > 0)
						message = string.Format(CultureInfo.CurrentCulture, format, args);

					source.TraceEvent(type, 0, message + Environment.NewLine + exception);
				}
			}
		}
	}
}
