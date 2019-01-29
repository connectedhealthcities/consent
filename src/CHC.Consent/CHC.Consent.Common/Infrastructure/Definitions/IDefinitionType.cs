namespace CHC.Consent.Common.Infrastructure.Definitions
{
    public interface IDefinitionType
    {
        void Accept(IDefinitionVisitor visitor, IDefinition definition);
        /// <summary>
        /// Lowercase name identifier
        /// </summary>
        string SystemName { get; }
    }
}