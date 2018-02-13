using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity
{
    public class SingleValueIdentifierTypeHelper<TIdentifierValue, TValue> : SingleValueIdentifierHelper<TValue>
        where TIdentifierValue : IIdentifierValue<TValue>
    {
        /// <inheritdoc />
        public SingleValueIdentifierTypeHelper(Expression<Func<Person, TValue>> property) : base(property)
        {
        }

        public Expression<Func<Person, bool>> GetMatchExpression(TIdentifierValue value)
        {
            return GetMatchExpression(value.Value);
        }

        public void Update(Person person, TIdentifierValue value)
        {
            Update(person, value.Value);
        }
    }
}