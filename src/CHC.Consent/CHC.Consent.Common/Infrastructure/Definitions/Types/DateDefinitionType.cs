namespace CHC.Consent.Common.Infrastructure.Definitions.Types
{
    public class DateDefinitionType : IDefinitionType
    {
        public const string DataType = "date";

        /// <inheritdoc />
        public void Accept(IDefinitionVisitor visitor, IDefinition definition) =>
            visitor.Visit(definition, this);

        /// <inheritdoc />
        public string SystemName => DataType;
    }
}