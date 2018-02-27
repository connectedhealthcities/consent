namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier(TypeName)]
    public class NhsNumberIdentifier : IIdentifier, ISingleValueIdentifier<string>
    {
        public const string TypeName = "uk.nhs.nhs-number";

        /// <inheritdoc />
        public NhsNumberIdentifier(string value = null) 
        {
            Value = value;
        }

        public string Value { get; set; }


        protected bool Equals(NhsNumberIdentifier other)
        {
            return string.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NhsNumberIdentifier) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }
}