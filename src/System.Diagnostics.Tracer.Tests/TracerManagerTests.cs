using Moq;
using Xunit;

namespace System.Diagnostics
{
    public class TracerManagerTests
    {
		[Fact]
		public void when_getting_source_then_can_retrieve_built_in_source ()
		{
			var manager = new TracerManager();
			var expected = PresentationTraceSources.MarkupSource;
			var actual = manager.GetSource("System.Windows.Markup");

			Assert.Same (expected, actual);
		}

		[Fact]
		public void when_tracing_aggregate_then_replaces_source_name_for_each_level ()
		{
			var globalListener = new Mock<TraceListener>();
			var fooListener = new Mock<TraceListener>();
			var barListener = new Mock<TraceListener>();

			var manager = new TracerManager();

			var tracer = manager.Get("Foo.Bar");

			manager.AddListener (Tracer.Configuration.GlobalSourceName, globalListener.Object);
			manager.AddListener ("Foo", fooListener.Object);
			manager.AddListener ("Foo.Bar", barListener.Object);

			manager.SetTracingLevel (Tracer.Configuration.GlobalSourceName, SourceLevels.All);
			manager.SetTracingLevel ("Foo", SourceLevels.All);
			manager.SetTracingLevel ("Foo.Bar", SourceLevels.All);

			tracer.Trace (TraceEventType.Information, "Hi");

			globalListener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo.Bar", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()));
			fooListener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo.Bar", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()));
			barListener.Verify (x => x.TraceEvent (It.IsAny<TraceEventCache> (), "Foo.Bar", TraceEventType.Information, It.IsAny<int> (), "Hi", It.IsAny<object[]> ()));
		}
	}
}
