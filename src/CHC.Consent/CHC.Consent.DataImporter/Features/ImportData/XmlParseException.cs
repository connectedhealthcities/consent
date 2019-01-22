using System;
using System.Xml;
using System.Xml.Linq;

namespace CHC.Consent.DataImporter.Features.ImportData
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

        /// <inheritdoc />
        public override string ToString()
        {
            return
                Message + GetLineInfo() + "\r\n" + base.StackTrace;
        }

        private string GetLineInfo()
        {
            if(!HasLineInfo) return string.Empty;
            var lineInfo = "";
            if (!string.IsNullOrEmpty(BaseUri))
            {
                lineInfo = $"{BaseUri} at ";
            }

            return lineInfo + $"{LineNumber}:{LinePosition}";
        }

        public string BaseUri { get; }

        public int LineNumber { get; }

        public int LinePosition { get; }

        public bool HasLineInfo { get; }
    }
}