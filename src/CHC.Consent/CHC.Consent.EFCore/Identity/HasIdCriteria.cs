using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.Identity
{
    public class HasIdCriteria : ICriteria<PersonEntity>
    {
        /// <inheritdoc />
        public HasIdCriteria(IEnumerable<long> ids)
        {
            PersonIds = ids.ToArray();
        }

        public HasIdCriteria(IEnumerable<PersonIdentity> ids) : this(ids.Select(_ => _.Id))
        {
        }
            
        private long[] PersonIds { get; }

        /// <inheritdoc />
        public IQueryable<PersonEntity> ApplyTo(IQueryable<PersonEntity> queryable, ConsentContext context) =>
            queryable.Where(p => PersonIds.Contains(p.Id));
    }
}