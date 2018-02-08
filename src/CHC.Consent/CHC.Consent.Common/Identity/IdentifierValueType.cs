namespace CHC.Consent.Common.Identity
{
    public abstract class IdentifierValueType
    {
        public abstract IdentifierValue Parse(string value);
    }

    public abstract class IdentifierValueType<TValue> : IdentifierValueType
        where TValue: IdentifierValue
    {
        /// <inheritdoc />
        public override IdentifierValue Parse(string value)
        {
            return ParseToValue(value);
        }

        public abstract TValue ParseToValue(string value);
    }
}