using System;
using System.Collections;
using System.Linq;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public class IdentifierValue : IEquatable<IdentifierValue>
    {
        public object Value { get; }

        public IdentifierValue(object value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public bool Equals(IdentifierValue other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Value is IEnumerable thisEnumerable && other.Value is IEnumerable otherEnumerable)
            {
                return thisEnumerable.Cast<object>().SequenceEqual(otherEnumerable.Cast<object>());
            }
            return Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentifierValue) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public static bool operator ==(IdentifierValue left, IdentifierValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IdentifierValue left, IdentifierValue right)
        {
            return !Equals(left, right);
        }
    }
}