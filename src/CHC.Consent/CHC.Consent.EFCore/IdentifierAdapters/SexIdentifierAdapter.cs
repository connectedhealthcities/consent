using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class SexIdentifierAdapter : 
        IIdentifierFilter<SexIdentifier>,
        IIdentifierRetriever<SexIdentifier>,
        IIdentifierUpdater<SexIdentifier>
    {
        public IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people,
            SexIdentifier value,
            IStoreProvider stores)
        {
            var marshalledValue = MarshalledValue(value);
            return people.Where(
                p => stores.Get<IdentifierEntity>().Any(
                    _ =>
                        _.Person == p &&
                        _.TypeName == SexIdentifier.TypeName
                        && _.Deleted == null &&
                        _.Value == marshalledValue));
        }

        private static string MarshalledValue(SexIdentifier value)
        {
            var marshalledValue = value.Sex?.ToString();
            return marshalledValue;
        }

        /// <inheritdoc />
        public IEnumerable<SexIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return  ExistingIdentifierEntities(person, stores)
                .AsEnumerable()
                .Select(_ => new SexIdentifier(Enum.TryParse<Sex>(_.Value, out var sex) ? sex : (Sex?)null));
        }

        private static IQueryable<IdentifierEntity> ExistingIdentifierEntities(PersonEntity person, IStoreProvider stores)
        {
            return stores.Get<IdentifierEntity>().Where(_ => _.Person == person && _.TypeName == SexIdentifier.TypeName && _.Deleted == null);
        }

        /// <inheritdoc />
        public bool Update(PersonEntity person, SexIdentifier value, IStoreProvider stores)
        {
            var existing = ExistingIdentifierEntities(person, stores).SingleOrDefault();
            if (existing != null && existing.Value != MarshalledValue(value))
            {
                existing.Deleted = DateTime.UtcNow;
            }

            stores.Get<IdentifierEntity>().Add(
                new IdentifierEntity
                {
                    Person = person,
                    TypeName = SexIdentifier.TypeName,
                    Value = MarshalledValue(value),
                    ValueType = "string"
                });

            return true;
        }
    }
}