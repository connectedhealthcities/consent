using CHC.Consent.Common.Infrastructure.Definitions;

namespace CHC.Consent.Common.Infrastructure
{
    public interface IIdentifier<TDefinition> where TDefinition:DefinitionBase
    {
        IIdentifierValue Value { get; set; }
        TDefinition Definition { get; set; }
    }
}