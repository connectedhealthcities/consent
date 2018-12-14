namespace CHC.Consent.Common.Identity.Identifiers
{
    public class DateIdentifierType : IIdentifierType
    {
        /// <inheritdoc />
        public void Accept(IIdentifierDefinitionVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}