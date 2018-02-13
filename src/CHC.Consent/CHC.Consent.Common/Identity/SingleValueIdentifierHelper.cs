using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity
{
    public class SingleValueIdentifierHelper<TValue>
    {
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

        /// <inheritdoc />
        public SingleValueIdentifierHelper(Expression<Func<Person, TValue>> property)
        {
            Property = property;
            Getter = property.Compile();
            var member = (MemberExpression)property.Body;
            Setter = MakeSetter(property, member);
            PropertyName = member.Member.Name;
        }

        public Expression<Func<Person, bool>> GetMatchExpression(TValue matchValue)
        {
            return Expression.Lambda<Func<Person, bool>>(
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    (MemberExpression) Property.Body,
                    Expression.Convert(Expression.Constant(matchValue), typeof(TValue))),
                Property.Parameters[0]
            );
        }

        public void Update(Person person, TValue updatedValue)
        {
            var existingValue = Getter(person);
            if (Equals(existingValue, default(TValue)))
            {
                Setter(person, updatedValue);
            }
            else if (!Equals(existingValue, updatedValue))
            {
                throw new InvalidOperationException($"Cannot change {PropertyName} for Person#{person.Id}");
            }
        }
    }
}