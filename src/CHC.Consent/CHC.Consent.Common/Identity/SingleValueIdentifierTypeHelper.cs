using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity
{
    public class SingleValueIdentifierTypeHelper<TIdentifierValue, TValue>
        where TIdentifierValue : IIdentifierValue<TValue>
    {
        /// <inheritdoc />
        public SingleValueIdentifierTypeHelper(Expression<Func<Person, TValue>> property)
        {
            Property = property;
            Getter = property.Compile();
            var member = (MemberExpression)property.Body;
            Setter = MakeSetter(property, member);
            PropertyName = member.Member.Name;
        }

        private static Action<Person, TValue> MakeSetter(Expression<Func<Person, TValue>> property, MemberExpression member)
        {
            var param = Expression.Parameter(typeof(TValue), "value");
            var set = Expression.Lambda<Action<Person, TValue>>(
                Expression.Assign(member, param),
                property.Parameters[0],
                param);

            var compile = set.Compile();
            return compile;
        }

        public string PropertyName { get;  }

        public Expression<Func<Person, TValue>> Property { get;  }

        public Action<Person, TValue> Setter { get; }

        public Func<Person, TValue> Getter { get; }

        public Expression<Func<Person, bool>> GetMatchExpression(TIdentifierValue value)
        {
            return Expression.Lambda<Func<Person, bool>>(
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    (MemberExpression) Property.Body,
                    Expression.Convert(Expression.Constant(value.Value), typeof(TValue))),
                Property.Parameters[0]
            );
        }

        public void Update(Person person, TIdentifierValue value)
        {
            var existingValue = Getter(person);
            var updatedValue = value.Value;
            if (Equals(existingValue, default(TValue)))
            {
                Setter(person, updatedValue);
            }
            else if(!Equals(existingValue, updatedValue))
            {
                throw new InvalidOperationException($"Cannot change {PropertyName} for Person#{person.Id}");
            }
        }
    }
}