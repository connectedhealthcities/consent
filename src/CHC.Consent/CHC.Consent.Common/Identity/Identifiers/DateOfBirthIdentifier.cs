using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier("date-of-birth")]
    public class DateOfBirthIdentifier : IIdentifier, ISingleValueIdentifier<DateTime?>
    {
        private static readonly SingleValueIdentifierHelper<DateTime?> Helper
            = new SingleValueIdentifierHelper<DateTime?>(_ => _.DateOfBirth);

        /// <inheritdoc />
        public DateOfBirthIdentifier(DateTime? dateOfBirth=null)
        {
            DateOfBirth = dateOfBirth?.Date;
        }
        
        public DateTime? DateOfBirth { get; set; }
        DateTime? ISingleValueIdentifier<DateTime?>.Value => DateOfBirth;

        public Expression<Func<Person, bool>> GetMatchExpression()
        {
            return Helper.GetMatchExpression(DateOfBirth);
        }

        public void Update(Person person)
        {
            Helper.Update(person, DateOfBirth);
        }
    }
}