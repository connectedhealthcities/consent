using Newtonsoft.Json;

namespace CHC.Consent.Common.Identity.Identifiers.Medway
{
    [Identifier(TypeName, AllowMultipleValues = false)]
    public class MedwayNameIdentifier : IPersonIdentifier
    {
        public const string TypeName = "uk.nhs.bradfordhospitals.bib4all.medway.name";

        /// <summary> For Xml</summary>
        protected MedwayNameIdentifier():this(null, null) {}
        
        /// <inheritdoc />
        public MedwayNameIdentifier(string firstName = null, string lastName = null)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }


        protected bool Equals(MedwayNameIdentifier other)
        {
            return string.Equals(FirstName, other.FirstName) && string.Equals(LastName, other.LastName);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MedwayNameIdentifier) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((FirstName != null ? FirstName.GetHashCode() : 0) * 397) ^ (LastName != null ? LastName.GetHashCode() : 0);
            }
        }
    }
}