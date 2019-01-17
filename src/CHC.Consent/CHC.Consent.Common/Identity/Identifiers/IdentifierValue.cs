using System;
using System.Collections;
using System.Linq;

namespace CHC.Consent.Common.Identity.Identifiers
{
    public interface IIdentifierValue
    {
        object Value { get; }
    }

    public class CompositeIdentifierValue<TIdentifier> : IIdentifierValue
    {
        public TIdentifier[] Identifiers { get; }

        /// <inheritdoc />
        public CompositeIdentifierValue(TIdentifier[] identifiers)
        {
            Identifiers = identifiers;
        }

        /// <inheritdoc />
        object IIdentifierValue.Value => Identifiers;


        /// <inheritdoc />
        public bool Equals(CompositeIdentifierValue<TIdentifier> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Identifiers.SequenceEqual(other.Identifiers);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CompositeIdentifierValue<TIdentifier>) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Identifiers != null ? Identifiers.GetHashCode() : 0);
        }

        public static bool operator ==(CompositeIdentifierValue<TIdentifier> left, CompositeIdentifierValue<TIdentifier> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CompositeIdentifierValue<TIdentifier> left, CompositeIdentifierValue<TIdentifier> right)
        {
            return !Equals(left, right);
        }
    }

    public class SimpleIdentifierValue : IIdentifierValue
    {
        public object Value { get; }

        public SimpleIdentifierValue(object value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public bool Equals(SimpleIdentifierValue other)
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
            return Equals((SimpleIdentifierValue) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public static bool operator ==(SimpleIdentifierValue left, SimpleIdentifierValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SimpleIdentifierValue left, SimpleIdentifierValue right)
        {
            return !Equals(left, right);
        }
    }
}