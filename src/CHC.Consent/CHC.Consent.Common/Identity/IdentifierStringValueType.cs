namespace CHC.Consent.Common.Identity
{
    public class IdentifierStringValueType : IdentifierValueType<IdentifierStringValue>
    {
        /// <inheritdoc />
        public override IdentifierStringValue ParseToValue(string value)
        {
            return new IdentifierStringValue(value);
        }

        protected bool Equals(IdentifierStringValueType other) => true;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentifierStringValueType) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode();

    }
}