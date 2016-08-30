using System.Reflection;

namespace System.Diagnostics
{
	/// <summary>
	/// The TraceSource instance name matches the name of each of the "segments"
	/// we built the aggregate source from. This means that when we trace, we issue
	/// multiple trace statements, one for each. If a listener is added to (say) "*"
	/// source name, all traces done through it will appear as coming from the source
	/// "*", rather than (say) "Foo.Bar" which might be the actual source class.
	/// This diminishes the usefulness of hierarchical loggers significantly, since
	/// it now means that you need to add listeners too all trace sources you're
	/// interested in receiving messages from, and all its "children" potentially,
	/// some of them which might not have been created even yet. This is not feasible.
	/// Instead, since we issue the trace call to each trace source (which is what
	/// enables the configurability of all those sources in the app.config file),
	/// we need to fix the source name right before tracing, so that a configured
	/// listener at "*" still receives as the source name the original (aggregate) one,
	/// and not "*". This requires some private reflection, and a lock to guarantee
	/// proper logging, but this decreases its performance.
	/// </summary>
	class SourceNameReplacer : IDisposable
	{
		// Private reflection needed here in order to make the inherited source names still
		// log as if the original source name was the one logging, so as not to lose the
		// originating class name.
		static readonly FieldInfo sourceNameField = typeof(TraceSource).GetTypeInfo().GetDeclaredField("sourceName");

		TraceSource source;
		string originalName;

		static Action<TraceSource, string> replacer;

		static SourceNameReplacer()
		{
			if (sourceNameField == null)
				replacer = (source, name) => { };
			else
				replacer = (source, name) => sourceNameField.SetValue(source, name);
		}

		public SourceNameReplacer(TraceSource source, string sourceName)
		{
			this.source = source;
			originalName = source.Name;
			// Transient change of the source name while the trace call
			// is issued, if possible.
			replacer(source, sourceName);
		}

		public void Dispose()
		{
			replacer(source, originalName);
		}
	}
}
