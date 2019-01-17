namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IntegerIdentifierType : IIdentifierType
    {
        /// <inheritdoc />
        public void Accept(IDefinitionVisitor visitor, IDefinition definition) =>
            visitor.Visit(definition, this);

        /// <inheritdoc />
        public string SystemName => "integer";
    }
}