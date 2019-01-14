using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Identity
{
    public interface IIdentifierXmlMarshaller
    {
        XElement MarshallToXml(PersonIdentifier identifier);
        PersonIdentifier MarshallFromXml(XElement xElement);
    }
}