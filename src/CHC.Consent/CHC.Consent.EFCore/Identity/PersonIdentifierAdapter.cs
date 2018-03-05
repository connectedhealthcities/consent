using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore.Identity
{
    /// <summary>
    /// Deals with storing, finding, and retrieving <see cref="IPersonIdentifier"/>
    /// </summary>
    public class PersonIdentifierAdapter<TIdentifier> : 
        IIdentifierFilter<TIdentifier>, 
        IIdentifierRetriever<TIdentifier>, 
        IIdentifierUpdater<TIdentifier> 
        where TIdentifier : IPersonIdentifier
    {
        private readonly IIdentifierMarshaller<TIdentifier> marshaller;
        private readonly string typeName;

        public PersonIdentifierAdapter(IIdentifierMarshaller<TIdentifier> marshaller, string typeName)
        {
            this.marshaller = marshaller;
            this.typeName = typeName;
        }

        public IQueryable<PersonEntity> Filter(IQueryable<PersonEntity> people,TIdentifier value,IStoreProvider stores)
        {
            var marshalledValue = marshaller.MarshalledValue(value);
            return people.Where(
                p => stores.Get<PersonIdentifierEntity>().Any(
                    _ =>
                        _.Person == p &&
                        _.TypeName == typeName
                        && _.Deleted == null &&
                        _.Value == marshalledValue));
        }

        /// <inheritdoc />
        public IEnumerable<TIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return  ExistingIdentifierEntities(person, stores)
                .AsEnumerable()
                .Select(_ => marshaller.Unmarshall(_.ValueType, _.Value) );
        }

        private IQueryable<PersonIdentifierEntity> ExistingIdentifierEntities(PersonEntity person, IStoreProvider stores)
        {
            return stores.Get<PersonIdentifierEntity>().Where(_ => _.Person == person && _.TypeName == typeName && _.Deleted == null);
        }

        /// <inheritdoc />
        public bool Update(PersonEntity person, TIdentifier value, IStoreProvider stores)
        {
            var existing = ExistingIdentifierEntities(person, stores).SingleOrDefault();
            var marshalledValue = marshaller.MarshalledValue(value);
            
            if (existing != null)
            {
                if (existing.Value == marshalledValue) return false;
                
                existing.Deleted = DateTime.UtcNow;
            }

            stores.Get<PersonIdentifierEntity>().Add(
                new PersonIdentifierEntity
                {
                    Person = person,
                    TypeName = typeName,
                    Value = marshalledValue,
                    ValueType = marshaller.ValueType
                });

            return true;
        }
    }
}