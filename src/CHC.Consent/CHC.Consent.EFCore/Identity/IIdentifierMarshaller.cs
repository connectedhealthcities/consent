using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Identity
{
    public interface IIdentifierMarshaller
    {
        XElement MarshallToXml(PersonIdentifier identifier);
        PersonIdentifier MarshallFromXml(XElement xElement);
    }
}