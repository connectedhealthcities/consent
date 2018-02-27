using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Common
{
    public class PersonIdentity 
    {
        public virtual long Id { get;  }

        /// <inheritdoc />
        public PersonIdentity(long id)
        {
            Id = id;
        }

        protected bool Equals(PersonIdentity other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PersonIdentity) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}