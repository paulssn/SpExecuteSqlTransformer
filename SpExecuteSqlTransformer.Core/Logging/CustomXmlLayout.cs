using log4net.Core;
using log4net.Layout;
using System.Xml;

namespace SpExecuteSqlTransformer.Core.Logging
{
    /// <summary>
    /// NOTE: Currently not used, because when using CustomXmlLayout, there was the following problem: specific log statements were missing, for example
    /// when exceptions were thrown right after the start. Worked with log4net XmlLayout and normal non-XML log file. Didn't investigate this any further.
    /// </summary>
    public class CustomXmlLayout : XmlLayoutBase
    {
        protected override void FormatXml(XmlWriter writer, LoggingEvent loggingEvent)
        {
            writer.WriteStartElement("LogEntry");

            //writer.WriteAttributeString("Timestamp", DateTime.Now.ToString("yyyyMMdd-HH:mm:sss.fff"));
            //writer.WriteAttributeString("Level", loggingEvent.Level.DisplayName);
            //writer.WriteAttributeString("Message", loggingEvent.RenderedMessage);

            //if (loggingEvent.ExceptionObject != null)
            //{
            //    writer.WriteAttributeString("Exception", loggingEvent.ExceptionObject.ToString());
            //    writer.WriteAttributeString("Stacktrace", loggingEvent.ExceptionObject.GetType().FullName);
            //}
            //writer.WriteAttributeString("Logger", loggingEvent.LoggerName);

            writer.WriteEndElement();
        }
    }
}
