using System.Linq;

namespace CHC.Consent.Common.Infrastructure
{
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
}