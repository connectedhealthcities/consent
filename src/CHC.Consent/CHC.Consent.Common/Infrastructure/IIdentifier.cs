using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Common.Infrastructure
{
    public interface IIdentifier<TDefinition> where TDefinition:DefinitionBase
    {
        IIdentifierValue Value { get; set; }
        TDefinition Definition { get; set; }
    }
}