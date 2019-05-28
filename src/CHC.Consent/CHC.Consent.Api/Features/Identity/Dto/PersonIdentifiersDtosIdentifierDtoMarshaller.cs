using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonIdentifiersDtosIdentifierDtoMarshaller : IdentifierDtoMarshaller<PersonIdentifier, IdentifierDefinition>
    {
        public PersonIdentifiersDtosIdentifierDtoMarshaller(DefinitionRegistry registry): base(registry, CreateDelegate)
        { 
        }

        private static readonly CreateIdentifier CreateDelegate = Create;  
        private static PersonIdentifier Create(IdentifierDefinition definition, IIdentifierValue value)
        {
            return new PersonIdentifier(value, definition);
        }
    }
}