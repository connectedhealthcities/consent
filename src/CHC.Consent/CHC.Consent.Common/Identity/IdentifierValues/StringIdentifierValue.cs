namespace CHC.Consent.Common.Identity.IdentifierValues
{
    public class StringIdentifierValue : IdentifierValue
    {
        public string Value { get; }

        /// <inheritdoc />
        public StringIdentifierValue(string value)
        {
            Value = value;
        }

        protected bool Equals(StringIdentifierValue other)
        {
            return string.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((StringIdentifierValue) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}