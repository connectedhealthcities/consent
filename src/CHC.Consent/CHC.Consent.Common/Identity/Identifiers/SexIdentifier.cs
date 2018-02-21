using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier("sex")]
    public class SexIdentifier : IIdentifier, ISingleValueIdentifier<Sex?>
    {
        private static readonly SingleValueIdentifierHelper<Sex?> Helper =
            new SingleValueIdentifierHelper<Sex?>(_ => _.Sex);

        /// <inheritdoc />
        public SexIdentifier(Sex? sex=null)
        {
            Sex = sex;
        }

        public Sex? Sex { get; set; }

        /// <inheritdoc />
        Sex? ISingleValueIdentifier<Sex?>.Value => this.Sex;

        /// <inheritdoc />
        public Expression<Func<Person, bool>> GetMatchExpression() => Helper.GetMatchExpression(Sex);

        /// <inheritdoc />
        public void Update(Person person) => Helper.Update(person, Sex);
    }
}