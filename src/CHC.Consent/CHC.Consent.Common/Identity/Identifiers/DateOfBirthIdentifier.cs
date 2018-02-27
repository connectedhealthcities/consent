using System;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier("date-of-birth")]
    public class DateOfBirthIdentifier : IIdentifier, ISingleValueIdentifier<DateTime?>
    {
        /// <inheritdoc />
        public DateOfBirthIdentifier(DateTime? dateOfBirth=null)
        {
            DateOfBirth = dateOfBirth?.Date;
        }
        
        public DateTime? DateOfBirth { get; set; }
        DateTime? ISingleValueIdentifier<DateTime?>.Value => DateOfBirth;

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