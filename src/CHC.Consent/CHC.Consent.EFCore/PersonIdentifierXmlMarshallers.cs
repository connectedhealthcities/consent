using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.EFCore.Identity;

namespace CHC.Consent.EFCore
{
    public interface IIdentifierEntity 
    {
        string TypeName { get; }
        string Value { get; }
    }
    public class IdentifierXmlMarshallers<TIdentifier, TDefinition> where TIdentifier : IIdentifier<TDefinition>, new()
        where TDefinition : DefinitionBase
    {
        public IDictionary<string, IIdentifierXmlMarhsaller<TIdentifier, TDefinition>> Marshallers { get; }

        /// <inheritdoc />
        public IdentifierXmlMarshallers(DefinitionRegistry registry)
        {
            Marshallers = registry.Accept(new IdentifierXmlMarshallerCreator<TIdentifier, TDefinition>()).Marshallers;
        }
        
        public XElement MarshallToXml(TIdentifier identifier)
        {
            return Marshallers[identifier.Definition.SystemName].MarshallToXml(identifier);
        }

        public TIdentifier MarshallFromXml(IIdentifierEntity entity)
        {
            return MarshallFromXml(entity.TypeName, XElement.Parse(entity.Value));
        }

        public TIdentifier MarshallFromXml(string typeName, XElement xElement)
        {
            return Marshallers[typeName].MarshallFromXml(xElement);
        }

        public IEnumerable<TIdentifier> MarshallFromXml(IEnumerable<IIdentifierEntity> identifierEntities)
        {
            return identifierEntities.Select(MarshallFromXml);
        }
    }
    
    public class PersonIdentifierXmlMarshallers : IdentifierXmlMarshallers<PersonIdentifier, IdentifierDefinition>
    {
        /// <inheritdoc />
        public PersonIdentifierXmlMarshallers(DefinitionRegistry registry) : base(registry)
        {
        }
    }
}