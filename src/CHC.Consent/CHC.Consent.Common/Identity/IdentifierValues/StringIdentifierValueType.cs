namespace CHC.Consent.Common.Identity.IdentifierValues
{
    public class StringIdentifierValueType : IdentifierValueType<StringIdentifierValue>
    {
        /// <inheritdoc />
        public override StringIdentifierValue ParseToValue(string value)
        {
            return new StringIdentifierValue(value);
        }

        protected bool Equals(StringIdentifierValueType other) => true;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StringIdentifierValueType) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode();

    }
}