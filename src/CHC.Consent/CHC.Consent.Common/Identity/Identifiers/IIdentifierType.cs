namespace CHC.Consent.Common.Identity.Identifiers
{
    public interface IIdentifierType
    {
        void Accept(IIdentifierDefinitionVisitor visitor);
        /// <summary>
        /// Lowercase name identifier
        /// </summary>
        string SystemName { get; }
    }
}