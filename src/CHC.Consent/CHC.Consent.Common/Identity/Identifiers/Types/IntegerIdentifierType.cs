namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IntegerIdentifierType : IIdentifierType
    {
        /// <inheritdoc />
        public void Accept(IIdentifierDefinitionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}