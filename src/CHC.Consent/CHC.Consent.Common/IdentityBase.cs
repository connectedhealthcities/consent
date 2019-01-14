using System;

namespace CHC.Consent.Common
{
    public abstract class IdentityBase
    {
        private static string _shortName;

        private static string GetShortName<T>(T instance) where T:IdentityBase
        {
            if (_shortName != null) return _shortName;
            
            _shortName = instance.GetType().Name;
            if (_shortName.EndsWith("Identity"))
            {
                _shortName = _shortName.Substring(0, _shortName.Length - "Identity".Length);
            }

            return _shortName;
        }
        
        public long Id { get; }

        /// <inheritdoc />
        protected IdentityBase(long id)
        {
            Id = id;
        }

        protected bool Equals(IdentityBase other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IdentityBase) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(IdentityBase left, IdentityBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IdentityBase left, IdentityBase right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetShortName(this) + "#" + Id;
        }

        public static implicit operator long(IdentityBase identity)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            return identity.Id;
        }
    }
}