using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class NhsNumberIdentifierAdapter : 
        IIdentifierFilter<NhsNumberIdentifier>,
        IIdentifierRetriever<NhsNumberIdentifier>,
        IIdentifierUpdater<NhsNumberIdentifier>
    {
        /// <inheritdoc />
        public IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people,
            NhsNumberIdentifier value,
            IStoreProvider stores) =>
            people.Where(
                p => stores.Get<IdentifierEntity>().Any(
                    _ => _.Person == p && _.TypeName == NhsNumberIdentifier.TypeName && _.Deleted == null &&
                         _.Value == value.Value));

        /// <inheritdoc />
        public IEnumerable<NhsNumberIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return  ExistingIdentifierEntities(person, stores)
                .Select(_ => new NhsNumberIdentifier(_.Value));
        }

        private static IQueryable<IdentifierEntity> ExistingIdentifierEntities(PersonEntity person, IStoreProvider stores)
        {
            return stores.Get<IdentifierEntity>().Where(_ => _.Person == person && _.TypeName == NhsNumberIdentifier.TypeName && _.Deleted == null);
        }

        /// <inheritdoc />
        public bool Update(PersonEntity person, NhsNumberIdentifier value, IStoreProvider stores)
        {
            var existing = ExistingIdentifierEntities(person, stores).SingleOrDefault();
            if (existing != null && existing.Value != value.Value)
            {
                existing.Deleted = DateTime.UtcNow;
            }

            stores.Get<IdentifierEntity>().Add(
                new IdentifierEntity
                {
                    Person = person,
                    TypeName = NhsNumberIdentifier.TypeName,
                    Value = value.Value,
                    ValueType = "string"
                });

            return true;
        }
    }
}