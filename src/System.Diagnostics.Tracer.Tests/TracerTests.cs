using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace System.Diagnostics
{
    public class TracerTests
    {
        [Fact]
        public void when_getting_source_name_from_type_then_retrieves_full_name()
        {
            var name = Tracer.NameFor<TracerTests>();

            Assert.Equal(typeof(TracerTests).FullName, name);
        }

        /// <summary>
        /// <see cref="System.Lazy{System.Tuple{String, int}}"/>.
        /// </summary>
        [Fact]
        public void when_getting_source_name_for_generics_type_then_retrieves_documentation_style_generic()
        {
            var name = Tracer.NameFor<Lazy<Tuple<string, int>>>();

            Assert.Equal("System.Lazy{System.Tuple{System.String,System.Int32}}", name);
        }

        [Fact]
        public void when_tracing_exception_with_xml_then_succeeds()
        {
            var xml = Path.GetTempFileName();
            var inMemoryListener = new InMemoryTraceListener();
            var xmlListener = new XmlWriterTraceListener(xml);

            Tracer.Configuration.AddListener("Foo", inMemoryListener);
            Tracer.Configuration.AddListener("Foo", xmlListener);
            Tracer.Configuration.SetTracingLevel("Foo", SourceLevels.All);

            var tracer = Tracer.Get("Foo");

            tracer.Error(new ApplicationException("Foo Error"), "A Foo Exception occurred");

            var trace = inMemoryListener.GetTrace();

            Assert.False(string.IsNullOrEmpty(trace));
            Assert.Contains("Foo Error", trace);
            Assert.Contains("A Foo Exception occurred", trace);
        }

        [Fact]
        public void when_tracing_exception_without_xml_then_succeeds()
        {
            var inMemoryListener = new InMemoryTraceListener();

            Tracer.Configuration.AddListener("Foo", inMemoryListener);
            Tracer.Configuration.SetTracingLevel("Foo", SourceLevels.All);

            var tracer = Tracer.Get("Foo");

            tracer.Error(new ApplicationException("Foo Error"), "A Foo Exception occurred");

            var trace = inMemoryListener.GetTrace();

            Assert.False(string.IsNullOrEmpty(trace));
            Assert.Contains("Foo Error", trace);
            Assert.Contains("A Foo Exception occurred", trace);
        }
    }

    class InMemoryTraceListener : TraceListener
    {
        readonly StringBuilder traceBuilder = new StringBuilder();

        public override void Write(string message)
        {
            traceBuilder.Append(message);
        }

        public override void WriteLine(string message)
        {
            traceBuilder.AppendLine(message);
        }

        public string GetTrace()
        {
            return traceBuilder.ToString();
        }
    }
}
