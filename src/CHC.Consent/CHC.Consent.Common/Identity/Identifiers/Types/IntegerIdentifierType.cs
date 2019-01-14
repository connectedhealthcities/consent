namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IntegerIdentifierType : IIdentifierType
    {
        /// <inheritdoc />
        public virtual void Accept(IIdentifierDefinitionVisitor visitor, IdentifierDefinition definition)
        {
            visitor.Visit( definition,this);
        }

        /// <inheritdoc />
        public string SystemName => "integer";
    }
}