using System;
using Newtonsoft.Json;


namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier(TypeName)]
    public class DateOfBirthIdentifier : IPersonIdentifier
    {
        public const string TypeName = "uk.nhs.bradfordhospitals.bib4all.medway.date-of-birth";

        /// <remarks>For XML serialization</remarks>
        protected DateOfBirthIdentifier() {}
        
        /// <inheritdoc />
        public DateOfBirthIdentifier(DateTime dateOfBirth)
        {
            DateOfBirth = dateOfBirth.Date;
        }
        
        [JsonProperty(Required = Required.Always)]
        public DateTime DateOfBirth { get; set; }

        protected bool Equals(DateOfBirthIdentifier other)
        {
            return DateOfBirth.Equals(other.DateOfBirth);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DateOfBirthIdentifier) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return DateOfBirth.GetHashCode();
        }
    }
}