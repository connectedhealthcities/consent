using System;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class PersonIdentifier : IPersonIdentifier, IEquatable<PersonIdentifier>
    {
        public IIdentifierValue Value { get; }
        public IdentifierDefinition Definition { get; }

        public PersonIdentifier(IIdentifierValue value, IdentifierDefinition definition)
        {
            Value = value;
            Definition = definition;
        }

        /// <inheritdoc />
        public bool Equals(PersonIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Value, other.Value) && Equals(Definition, other.Definition);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PersonIdentifier) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ (Definition != null ? Definition.GetHashCode() : 0);
            }
        }

        public static bool operator ==(PersonIdentifier left, PersonIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PersonIdentifier left, PersonIdentifier right)
        {
            return !Equals(left, right);
        }
    }
}