namespace System.Diagnostics
{
	/// <summary>
	/// Manages <see cref="ITracer"/> instances. Provides the implementation
	/// for the <see cref="Tracer"/> static facade class.
	/// </summary>
	///	<nuget id="Tracer.Interfaces" />
	public interface ITracerManager
	{
		/// <summary>
		/// Gets a tracer instance with the specified name.
		/// </summary>
		ITracer Get(string name);
	}
}
