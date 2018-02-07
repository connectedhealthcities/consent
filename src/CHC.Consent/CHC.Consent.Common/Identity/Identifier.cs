using System.Dynamic;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common
{
    public class Identifier
    {
        public int Id { get; protected set; }
        public IdentifierType Type { get; protected set; }
        public IdentifierValueType ValueType { get; protected set; }
        public IdentifierValue Value { get; protected set; }

        /// <inheritdoc />
        protected Identifier(int id, IdentifierType type, IdentifierValueType valueType)
        {
            Id = id;
            Type = type;
            ValueType = valueType;
        }

        /// <inheritdoc />
        public Identifier(IdentifierType type, IdentifierValueType valueType, IdentifierValue value)
        {
            Type = type;
            ValueType = valueType;
            Value = value;
        }

        private bool Equals(Identifier other)
        {
            return Equals(Type, other.Type) && Equals(ValueType, other.ValueType) && Equals(Value, other.Value);
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
                var hashCode = (Type != null ? Type.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ValueType != null ? ValueType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
    
    public class IdentifierStringValue : IdentifierValue
    {
        public string Value { get; }

        /// <inheritdoc />
        public IdentifierStringValue(string value)
        {
            Value = value;
        }

        protected bool Equals(IdentifierStringValue other)
        {
            return string.Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((IdentifierStringValue) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }
    }

    public abstract class IdentifierValue
    {
    }
}