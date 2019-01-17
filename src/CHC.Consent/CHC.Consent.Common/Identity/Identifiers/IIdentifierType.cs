namespace CHC.Consent.Common.Identity.Identifiers
{
    public interface IIdentifierType
    {
        void Accept(IDefinitionVisitor visitor, IDefinition definition);
        /// <summary>
        /// Lowercase name identifier
        /// </summary>
        string SystemName { get; }
    }
}