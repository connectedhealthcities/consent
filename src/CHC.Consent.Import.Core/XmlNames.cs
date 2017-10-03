using System.Xml.Linq;

namespace CHC.Consent.Import.Core
{
    public class XmlNames
    {
        public const string ChcStandardDataNamespace = "urn:chc:consent:standard-data:v0.1";
        public static readonly XNamespace ChcNs = XmlNames.ChcStandardDataNamespace;
        public static readonly XName Value = ChcNs + "value";
    }
}