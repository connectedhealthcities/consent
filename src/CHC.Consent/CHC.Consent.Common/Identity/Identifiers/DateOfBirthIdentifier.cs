using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier("date-of-birth")]
    public class DateOfBirthIdentifier : IIdentifier
    {
        private static readonly SingleValueIdentifierHelper<DateTime> Helper
            = new SingleValueIdentifierHelper<DateTime>(_ => _.DateOfBirth);

        /// <inheritdoc />
        public DateOfBirthIdentifier()
        {
        }
        
        public DateTime DateOfBirth { get; set; }

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