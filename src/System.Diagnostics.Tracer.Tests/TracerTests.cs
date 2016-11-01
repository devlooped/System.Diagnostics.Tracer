﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

			var tracerName = MethodBase.GetCurrentMethod().Name;

			Tracer.Configuration.AddListener(tracerName, inMemoryListener);
            Tracer.Configuration.AddListener(tracerName, xmlListener);
            Tracer.Configuration.SetTracingLevel(tracerName, SourceLevels.All);

            var tracer = Tracer.Get(tracerName);

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

		[Fact]
		public void when_tracing_exception_without_args_and_message_with_brackets_then_success()
		{
			var inMemoryListener = new InMemoryTraceListener();

			Tracer.Configuration.AddListener("Foo", inMemoryListener);
			Tracer.Configuration.SetTracingLevel("Foo", SourceLevels.All);

			var tracer = Tracer.Get("Foo");

			var message = "A Foo Exception occurred {1223445}";
			tracer.Error(new ApplicationException("Foo Error"), message);

			var trace = inMemoryListener.GetTrace();

			Assert.False(string.IsNullOrEmpty(trace));
			Assert.Contains("Foo Error", trace);
			Assert.Contains(message, trace);
		}


		[Fact]
		public void when_tracing_without_args_then_success()
		{
			var xml = Path.GetTempFileName();
			var inMemoryListener = new InMemoryTraceListener();
			var xmlListener = new XmlWriterTraceListener(xml);

			var tracerName = MethodBase.GetCurrentMethod().Name;

			Tracer.Configuration.AddListener(tracerName, inMemoryListener);
			Tracer.Configuration.AddListener(tracerName, xmlListener);
			Tracer.Configuration.SetTracingLevel(tracerName, SourceLevels.All);

			var tracer = Tracer.Get(tracerName);

			var message = "Foo";

			tracer.Info(message);

			var trace = inMemoryListener.GetTrace();

			Assert.False(string.IsNullOrEmpty(trace));
			Assert.Contains(message, trace);
		}

		[Fact]
		public void when_tracing_without_args_and_message_with_brackets_then_success()
		{
			var xml = Path.GetTempFileName();
			var inMemoryListener = new InMemoryTraceListener();
			var xmlListener = new XmlWriterTraceListener(xml);

			var tracerName = MethodBase.GetCurrentMethod().Name;

			Tracer.Configuration.AddListener(tracerName, inMemoryListener);
			Tracer.Configuration.AddListener(tracerName, xmlListener);
			Tracer.Configuration.SetTracingLevel(tracerName, SourceLevels.All);

			var tracer = Tracer.Get(tracerName);

			var message = "Foo: {1234567890}";

			tracer.Info(message);

			var trace = inMemoryListener.GetTrace();

			Assert.False(string.IsNullOrEmpty(trace));
			Assert.Contains(message, trace);
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
