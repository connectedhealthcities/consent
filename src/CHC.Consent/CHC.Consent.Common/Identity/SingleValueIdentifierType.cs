using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity
{
    public abstract class SingleValueIdentifierType<TIdentifierValue, TIdentifierValueType, TValue> :
        IdentifierType<TIdentifierValue> 
        where TIdentifierValue : IdentifierValue, IIdentifierValue<TValue>
    
        where TIdentifierValueType: IdentifierValueType<TIdentifierValue>, new()
    {
        private readonly SingleValueIdentifierTypeHelper<IIdentifierValue<TValue>, TValue> helper;

        /// <inheritdoc />
        protected SingleValueIdentifierType(string externalId, Expression<Func<Person, TValue>> property) : 
            base(externalId, canHaveMultipleValues:false, valueType:new TIdentifierValueType())
        {
            helper = new SingleValueIdentifierTypeHelper<IIdentifierValue<TValue>, TValue>(property);
        }

        /// <inheritdoc />
        protected override Expression<Func<Person, bool>> GetMatchExpression(TIdentifierValue value)
            => helper.GetMatchExpression(value);


        /// <inheritdoc />
        protected override void Update(Person person, TIdentifierValue value)
            => helper.Update(person, value);
    }
}