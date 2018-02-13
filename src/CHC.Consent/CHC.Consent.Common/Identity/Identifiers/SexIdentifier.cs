using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class SexIdentifier : IIdentifier
    {
        private static readonly SingleValueIdentifierHelper<Sex?> Helper =
            new SingleValueIdentifierHelper<Sex?>(_ => _.Sex);
        
        public Sex? Sex { get; set; }

        /// <inheritdoc />
        public Expression<Func<Person, bool>> GetMatchExpression() => Helper.GetMatchExpression(Sex);

        /// <inheritdoc />
        public void Update(Person person) => Helper.Update(person, Sex);
    }
}