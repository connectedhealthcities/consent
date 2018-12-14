namespace CHC.Consent.Common.Identity.Identifiers
{
    public class CompositeIdentifierType : IIdentifierType
    {
        public IdentifierDefinitionRegistry Identifiers { get; }

        public CompositeIdentifierType(params IdentifierDefinition[] identifiers)
        {
            Identifiers = new IdentifierDefinitionRegistry(identifiers);
        }

        /// <inheritdoc />
        public void Accept(IIdentifierDefinitionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}