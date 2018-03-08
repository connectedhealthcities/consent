namespace CHC.Consent.Common.Identity.Identifiers.Medway
{
    [Identifier(TypeName)]
    public class MedwaySexIdentifier : IPersonIdentifier
    {
        public const string TypeName = "uk.nhs.bradfordhospitals.bib4all.medway.sex";

        /// <inheritdoc />
        public MedwaySexIdentifier(Sex? sex=null)
        {
            Sex = sex;
        }

        public Sex? Sex { get; set; }

        protected bool Equals(MedwaySexIdentifier other)
        {
            return Sex == other.Sex;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MedwaySexIdentifier) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Sex.GetHashCode();
        }
    }
}