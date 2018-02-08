using System;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity
{
    public class Identifier
    {
        
        public IdentifierType IdentifierType { get; }
        public IdentifierValueType ValueType { get; }
        public IdentifierValue Value { get; }

        /// <inheritdoc />
        public Identifier(IdentifierType identifierType, IdentifierValueType valueType, IdentifierValue value)
        {
            IdentifierType = identifierType;
            ValueType = valueType;
            Value = value;
        }

        private bool Equals(Identifier other)
        {
            return Equals(IdentifierType, other.IdentifierType) && Equals(ValueType, other.ValueType) && Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Identifier other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (IdentifierType != null ? IdentifierType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ValueType != null ? ValueType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                return hashCode;
            }
        }

        public virtual Expression<Func<Person, bool>> GetMatchExpression()
        {
            return IdentifierType.GetMatchExpression(Value);
        }

        public virtual void Update(Person person)
        {
            IdentifierType.Update(person, Value);
        }
    }
}