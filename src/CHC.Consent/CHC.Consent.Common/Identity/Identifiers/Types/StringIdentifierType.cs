namespace CHC.Consent.Common.Identity.Identifiers
{
    public class StringIdentifierType : IIdentifierType 
    {
        /// <inheritdoc />
        public virtual void Accept(IIdentifierDefinitionVisitor visitor, IdentifierDefinition definition)
        {
            visitor.Visit( definition,this);
        }

        /// <inheritdoc />
        public string SystemName => "string";

        /// <inheritdoc />
        public IdentifierParseResult Parse(string value) 
            => IdentifierParseResult.Success(value);
    }
}