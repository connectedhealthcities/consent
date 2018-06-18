using System;
using System.Runtime.CompilerServices;
using CHC.Consent.Common;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Security;

[assembly:InternalsVisibleTo("CHC.Consent.EFCore.Tests")]

namespace CHC.Consent.EFCore.Entities
{
    /// <summary>
    /// Stored (and allocates) Ids for people
    /// </summary>
    public class PersonEntity : IEntity, ISecurable
    {
        public virtual long Id { get; set; }
        
        public static implicit operator PersonIdentity(PersonEntity entity)
        {
            return entity == null ? null : new PersonIdentity( entity.Id );
        }

        private bool Equals(PersonEntity other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PersonEntity) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public AccessControlList ACL { get; set; } = new AccessControlList { Description = "Person"};
    }
}