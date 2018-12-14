namespace CHC.Consent.Common.Identity.Identifiers
{
    public class StringIdentifierType : IIdentifierType {
        /// <inheritdoc />
        public void Accept(IIdentifierDefinitionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}