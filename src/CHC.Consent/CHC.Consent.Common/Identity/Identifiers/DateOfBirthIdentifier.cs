using System;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier(TypeName)]
    public class DateOfBirthIdentifier : IPersonIdentifier
    {
        public const string TypeName = "date-of-birth";

        /// <remarks>For XML serialization</remarks>
        protected DateOfBirthIdentifier() {}
        
        /// <inheritdoc />
        public DateOfBirthIdentifier(DateTime? dateOfBirth=null)
        {
            DateOfBirth = dateOfBirth?.Date;
        }
        
        public DateTime? DateOfBirth { get; set; }

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