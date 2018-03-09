using System;
using System.Xml;
using System.Xml.Linq;

namespace CHC.Consent.DataImporter
{
    public class XmlParseException : Exception
    {
        /// <inheritdoc />
        public XmlParseException(XObject source, string message) : base(message)
        {
            var lineInfo = ((IXmlLineInfo) source);
            BaseUri = source.BaseUri;
            LineNumber = lineInfo.LineNumber;
            LinePosition = lineInfo.LinePosition;
            HasLineInfo = lineInfo.HasLineInfo();
        }

        public string BaseUri { get; }

        public int LineNumber { get; }

        public int LinePosition { get; }

        public bool HasLineInfo { get; }
    }
}