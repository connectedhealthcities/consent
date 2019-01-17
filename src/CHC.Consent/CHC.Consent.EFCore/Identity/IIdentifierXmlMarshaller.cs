using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.EFCore.Identity
{
    public interface IIdentifierXmlMarhsaller<TIdentifier, TDefintion>
        where TIdentifier : IIdentifier<TDefintion>, new()
        where TDefintion : DefinitionBase
    {
        XElement MarshallToXml(TIdentifier identifier);
        TIdentifier MarshallFromXml(XElement xElement);
    }
    
    
}