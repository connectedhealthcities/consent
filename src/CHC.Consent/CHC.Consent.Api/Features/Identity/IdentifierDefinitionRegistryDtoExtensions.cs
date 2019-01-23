using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Api.Features.Identity
{
    public static class IdentifierDefinitionRegistryDtoExtensions
    {
        public static bool IsValidIdentifierType(
            this IdentifierDefinitionRegistry identifierDefinitionRegistry,
            IIdentifierValueDto identifier)
        {
            return identifierDefinitionRegistry.ContainsKey(identifier.SystemName);
        }
    }
}