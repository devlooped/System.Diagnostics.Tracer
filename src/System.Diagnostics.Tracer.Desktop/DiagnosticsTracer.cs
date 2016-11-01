using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

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
                    // Transfers with a Guid payload should instead trace a transfer
                    // with that as the related Guid.
                    var guid = message as Guid?;
                    if (guid != null && type == TraceEventType.Transfer)
                        source.TraceTransfer(0, "Transfer", guid.Value);
                    else
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

                    var hasXmlListeners = source.Listeners.OfType<XmlWriterTraceListener>().Any();
                    var xmlListeners = default(XmlWriterTraceListener[]);

                    try
                    {
                        if (hasXmlListeners)
                        {
                            TraceExceptionXml(exception, message);

                            // This means we've traced the exception as XML to at least one XML writer,
                            // So we should skip the next event from them.
                            xmlListeners = source.Listeners.OfType<XmlWriterTraceListener>().ToArray();

                            foreach (var listener in xmlListeners)
                            {
                                source.Listeners.Remove(listener);
                            }
                        }

                        source.TraceEvent(type, 0, message + Environment.NewLine + exception);
                    }
                    finally
                    {
                        if (hasXmlListeners)
                        {
                            source.Listeners.AddRange(xmlListeners);
                        }
                    }
                }
            }
        }

        const string TraceXmlNs = "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord";

        void TraceExceptionXml(Exception exception, string format, params object[] args)
        {
            var message = format;
            if (args != null && args.Length > 0)
                message = string.Format(CultureInfo.CurrentCulture, format, args);

            var xdoc = new XDocument();
            var writer = xdoc.CreateWriter();

            writer.WriteStartElement("", "TraceRecord", TraceXmlNs);
            writer.WriteAttributeString("Severity", "Error");
            //writer.WriteElementString ("TraceIdentifier", msdnTraceCode);
            writer.WriteElementString("Description", TraceXmlNs, message);
            writer.WriteElementString("AppDomain", TraceXmlNs, AppDomain.CurrentDomain.FriendlyName);
            writer.WriteElementString("Source", TraceXmlNs, source.Name);
            AddExceptionXml(writer, exception);
            writer.WriteEndElement();

            writer.Flush();
            writer.Close();

            source.TraceData(TraceEventType.Error, 0, xdoc.CreateNavigator());
        }

        static void AddExceptionXml(XmlWriter xml, Exception exception)
        {
            xml.WriteElementString("ExceptionType", TraceXmlNs, exception.GetType().AssemblyQualifiedName);
            xml.WriteElementString("Message", TraceXmlNs, exception.Message);
            xml.WriteElementString("StackTrace", TraceXmlNs, exception.StackTrace);
            xml.WriteElementString("ExceptionString", TraceXmlNs, exception.ToString());

            if ((exception.Data != null) && (exception.Data.Count > 0))
            {
                xml.WriteStartElement("DataItems", TraceXmlNs);
                foreach (var key in exception.Data.Keys)
                {
                    var data = exception.Data[key];
                    xml.WriteElementString(key.ToString(), data != null ? data.ToString() : "");
                }
                xml.WriteEndElement();
            }

            if (exception.InnerException != null)
            {
                xml.WriteStartElement("InnerException", TraceXmlNs);
                AddExceptionXml(xml, exception.InnerException);
                xml.WriteEndElement();
            }
        }
    }
}
