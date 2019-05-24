using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using FluentAssertions.Collections;

namespace CHC.Consent.EFCore.Tests
{
    public static class PersonIdentifierCollectionAssertions
    {
        public static AndWhichConstraint<GenericCollectionAssertions<PersonIdentifierEntity>, PersonIdentifierEntity>
            ContainSingleIdentifierValue(
                this GenericCollectionAssertions<PersonIdentifierEntity> assertions,
                IdentifierDefinition definition,
                string value,
                bool deleted = false
            ) => assertions.ContainSingleMarshalledIdentifierValue(definition, MarshallValue(definition, value), deleted);

        private static AndWhichConstraint<GenericCollectionAssertions<PersonIdentifierEntity>, PersonIdentifierEntity>
            ContainSingleMarshalledIdentifierValue(
                this GenericCollectionAssertions<PersonIdentifierEntity> assertions, 
                IDefinition definition,
                string marshalledValue, 
                bool deleted)
        {
            return assertions.ContainSingle(
                _ => _.Value == marshalledValue && _.TypeName == definition.SystemName &&
                     (deleted ? _.Deleted != null : _.Deleted == null));
        }

        private static string MarshallValue(IdentifierDefinition definition, string value)
        {
            return definition.CreateXmlMarshaller()
                .MarshallToXml(Identifiers.PersonIdentifier(value, definition))
                .ToString(SaveOptions.DisableFormatting);
        }

        public static IIdentifierXmlMarhsaller<PersonIdentifier, IdentifierDefinition> CreateXmlMarshaller(this IdentifierDefinition definition)
        {
            var creator = new IdentifierXmlMarshallerCreator<PersonIdentifier, IdentifierDefinition>();
            definition.Accept(creator);
            return creator.Marshallers.Values.Single();
        }

        public static AndWhichConstraint<GenericCollectionAssertions<PersonIdentifierEntity>, PersonIdentifierEntity>
            ContainSingleIdentifierValue(
                this GenericCollectionAssertions<PersonIdentifierEntity> assertions,
                PersonIdentifier identifier,
                bool deleted = false
            )
        {
            return assertions.ContainSingleMarshalledIdentifierValue(
                identifier.Definition,
                identifier.Definition.CreateXmlMarshaller().MarshallToXml(identifier)
                    .ToString(SaveOptions.DisableFormatting),
                deleted);
        }
    }
}