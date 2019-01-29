namespace CHC.Consent.Common.Infrastructure.Definitions
{
    public interface IDefinition
    {
        string SystemName { get; }
        IDefinitionType Type { get; }
    }
}