using System;
using System.Runtime.CompilerServices;
using CHC.Consent.Common;
using CHC.Consent.Common.Infrastructure.Data;

[assembly:InternalsVisibleTo("CHC.Consent.EFCore.Tests")]

namespace CHC.Consent.EFCore.Entities
{
    public class PersonEntity : IEntity
    {
        public virtual long Id { get; set; }
        public virtual string NhsNumber { get; set; }
        public virtual DateTime? DateOfBirth { get; set; }
        public virtual Sex? Sex { get; set; }
        
        public virtual ushort? BirthOrder
        {
            get => (ushort?) BirthOrderValue;
            set => BirthOrderValue = value;
        }

        internal int? BirthOrderValue { get; set; }


        public static implicit operator PersonIdentity(PersonEntity entity)
        {
            return entity == null ? null : new PersonIdentity( entity.Id );
        }
    }
}