using System.Xml.Linq;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;

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