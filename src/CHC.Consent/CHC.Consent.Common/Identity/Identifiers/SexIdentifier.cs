namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier("sex")]
    public class SexIdentifier : IIdentifier, ISingleValueIdentifier<Sex?>
    {
        /// <inheritdoc />
        public SexIdentifier(Sex? sex=null)
        {
            Sex = sex;
        }

        public Sex? Sex { get; set; }

        /// <inheritdoc />
        Sex? ISingleValueIdentifier<Sex?>.Value => this.Sex;

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