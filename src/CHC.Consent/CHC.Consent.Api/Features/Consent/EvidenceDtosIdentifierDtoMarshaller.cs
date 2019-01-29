using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.Api.Features.Consent
{
    public class EvidenceDtosIdentifierDtoMarshaller : IdentifierDtoMarshaller<Evidence, EvidenceDefinition>
    {
        public EvidenceDtosIdentifierDtoMarshaller(DefinitionRegistry registry) : base(registry, CreateDelegate)
        { 
        }

        private static readonly CreateIdentifier CreateDelegate = Create;  
        private static Evidence Create(EvidenceDefinition definition, IIdentifierValue value)
        {
            return new Evidence(definition, value);
        }
    }
}