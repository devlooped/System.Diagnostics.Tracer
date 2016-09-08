using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Diagnostics
{
	class TracerManager : ITracerManager, ITracerConfiguration
	{
		const string globalSourceName = "*";
		static readonly TraceSource globalSource;

		// If we can grab this from reflection from the TraceSource, we do it,
		// and it means we can also manipulate the configuration for built-in
		// sources instantiated natively by the BCLs throughout .NET :).
		static List<WeakReference> traceSourceCache = new List<WeakReference>();

		public string GlobalSourceName { get { return globalSourceName; } }

		static TracerManager()
		{
			var cacheField = typeof(TraceSource).GetTypeInfo().GetDeclaredField("tracesources");
			if (cacheField != null)
			{
				try
				{
					traceSourceCache = (List<WeakReference>)cacheField.GetValue(null);
				}
				catch { }
			}

			globalSource = CreateSource(globalSourceName);
		}

		public ITracer Get(string name)
		{
			return new AggregateTracer(name, CompositeFor(name)
				.Select(tracerName => new DiagnosticsTracer(
					this.GetOrAdd(tracerName, sourceName => CreateSource(sourceName)))));
		}

		public TraceSource GetSource(string name)
		{
			return GetOrAdd(name, sourceName => CreateSource(sourceName));
		}

		public void AddListener(string sourceName, TraceListener listener)
		{
			GetOrAdd(sourceName, name => CreateSource(name)).Listeners.Add(listener);
		}

		public void RemoveListener(string sourceName, TraceListener listener)
		{
			GetOrAdd(sourceName, name => CreateSource(name)).Listeners.Remove(listener);
		}

		public void RemoveListener(string sourceName, string listenerName)
		{
			GetOrAdd(sourceName, name => CreateSource(name)).Listeners.Remove(listenerName);
		}

		public void SetTracingLevel(string sourceName, SourceLevels level)
		{
			GetOrAdd(sourceName, name => CreateSource(name)).Switch.Level = level;
		}

		static TraceSource CreateSource(string name)
		{
			var source = new TraceSource(name);
			// The source.Listeners.Count call causes the tracer to be initialized from config at this point.
			source.TraceInformation("Initialized trace source {0} with initial level {1} and {2} initial listeners.", name, source.Switch.Level, source.Listeners.Count);
			
			return source;
		}

		/// <summary>
		/// Gets the list of trace source names that are used to inherit trace source logging for the given <paramref name="name"/>.
		/// </summary>
		static IEnumerable<string> CompositeFor(string name)
		{
			if (name != globalSourceName)
				yield return globalSourceName;

			var indexOfGeneric = name.IndexOf('<');
			var indexOfLastDot = name.LastIndexOf('.');

			if (indexOfGeneric == -1 && indexOfLastDot == -1)
			{
				yield return name;
				yield break;
			}

			var parts = default(string[]);

			if (indexOfGeneric == -1)
				parts = name
					.Substring(0, name.LastIndexOf('.'))
					.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			else
				parts = name
					.Substring(0, indexOfGeneric)
					.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 1; i <= parts.Length; i++)
			{
				yield return string.Join(".", parts, 0, i);
			}

			yield return name;
		}

		TraceSource GetOrAdd(string sourceName, Func<string, TraceSource> factory)
		{
			if (sourceName == globalSourceName)
				return globalSource;

			var cachedSource = traceSourceCache
				.ToArray()
				.Where(weak => weak.IsAlive)
				.Select(weak => (TraceSource)weak.Target)
				.FirstOrDefault(source => source != null && source.Name == sourceName);

			return cachedSource ?? factory(sourceName);
		}
	}
}
