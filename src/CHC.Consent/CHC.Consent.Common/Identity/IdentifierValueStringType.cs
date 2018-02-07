namespace CHC.Consent.Common.Identity
{
    public class IdentifierValueStringType : IdentifierValueType
    {
        /// <inheritdoc />
        public override Identifier Parse(string value, IdentifierType identifierType)
        {
            return new Identifier(identifierType, this, new IdentifierStringValue(value));
        }

        protected bool Equals(IdentifierValueStringType other) => true;

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentifierValueStringType) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => base.GetHashCode();

    }
}