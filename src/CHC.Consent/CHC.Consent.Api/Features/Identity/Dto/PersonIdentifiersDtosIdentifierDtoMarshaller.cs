using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonIdentifiersDtosIdentifierDtoMarshaller : IdentifierDtoMarshaller<PersonIdentifier, IdentifierDefinition>
    {
        public PersonIdentifiersDtosIdentifierDtoMarshaller(DefinitionRegistry registry): base(registry, CreateDelegate)
        { 
        }
        
        public static readonly CreateIdentifier CreateDelegate = Create;  
        private static PersonIdentifier Create(IdentifierDefinition definition, IIdentifierValue value)
        {
            return new PersonIdentifier(value, definition);
        }
    }
}