namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IntegerIdentifierType : IIdentifierType
    {
        /// <inheritdoc />
        public virtual void Accept(IIdentifierDefinitionVisitor visitor)
        {
            visitor.Visit(this);
        }

        /// <inheritdoc />
        public string SystemName => "integer";
    }
}