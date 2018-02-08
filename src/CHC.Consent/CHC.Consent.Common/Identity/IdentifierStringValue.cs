namespace CHC.Consent.Common
{
    public class IdentifierStringValue : IdentifierValue
    {
        public string Value { get; }

        /// <inheritdoc />
        public IdentifierStringValue(string value)
        {
            Value = value;
        }

        protected bool Equals(IdentifierStringValue other)
        {
            return string.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((IdentifierStringValue) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}