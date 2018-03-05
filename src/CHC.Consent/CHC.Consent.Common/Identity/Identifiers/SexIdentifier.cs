namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier(TypeName)]
    public class SexIdentifier : IPersonIdentifier
    {
        public const string TypeName = "sex";

        /// <inheritdoc />
        public SexIdentifier(Sex? sex=null)
        {
            Sex = sex;
        }

        public Sex? Sex { get; set; }

        protected bool Equals(SexIdentifier other)
        {
            return Sex == other.Sex;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SexIdentifier) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Sex.GetHashCode();
        }
    }
}