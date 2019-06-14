using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.EFCore.Identity;

namespace CHC.Consent.EFCore
{
    public class PersonIdentifierXmlMarshallers : IdentifierXmlMarshallers<PersonIdentifier, IdentifierDefinition>
    {
        /// <inheritdoc />
        public PersonIdentifierXmlMarshallers(DefinitionRegistry registry) : base(registry)
        {
        }
    }
}