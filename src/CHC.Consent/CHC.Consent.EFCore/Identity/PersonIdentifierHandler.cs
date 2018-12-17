using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CHC.Consent.EFCore.Identity
{
    /// <summary>
    /// Deals with storing, finding, and retrieving <see cref="IPersonIdentifier"/>
    /// </summary>
    public class PersonIdentifierPersistanceHandler<TIdentifier> : IPersonIdentifierPersistanceHandler<TIdentifier> where TIdentifier : IPersonIdentifier
    {
        private ILogger Logger { get; }
        private readonly IDictionary<string, IIdentifierMarshaller> marshaller;
        private readonly string typeName;

        public PersonIdentifierPersistanceHandler(IIdentifierMarshaller marshaller, string typeName, ILogger<PersonIdentifierPersistanceHandler<TIdentifier>> logger)
        {
            Logger = logger;
            throw new NotImplementedException();
        }

        public IQueryable<PersonEntity> Filter(IQueryable<PersonEntity> people,TIdentifier value,IStoreProvider stores)
        {
            throw new NotImplementedException();
            /*var marshalledValue = marshaller.MarshalledValue(value);
            return people.Where(
                p => stores.Get<PersonIdentifierEntity>().Any(
                    _ =>
                        _.Person == p &&
                        _.TypeName == typeName
                        && _.Deleted == null &&
                        _.Value == marshalledValue));*/
        }

        /// <inheritdoc />
        public IEnumerable<TIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            throw new NotImplementedException();
            /*return  ExistingIdentifierEntities(person, stores)
                .AsEnumerable()
                .Select(_ => marshaller.Unmarshall(_.ValueType, _.Value) );*/
        }

        private IQueryable<PersonIdentifierEntity> ExistingIdentifierEntities(PersonEntity person, IStoreProvider stores)
        {
            return stores.Get<PersonIdentifierEntity>().Where(_ => _.Person == person && _.TypeName == typeName && _.Deleted == null);
        }

        /// <inheritdoc />
        public bool Update(PersonEntity person, TIdentifier[] identifiers, IStoreProvider stores)
        {
            throw new NotImplementedException();
/*
            using (Logger.BeginScope(person))
            {
                var existing = ExistingIdentifierEntities(person, stores).ToList();
                var changed = false;
                foreach (var existingId in existing)
                {
                    if (identifiers.Any(_ => existingId.Value == marshaller.MarshalledValue(_))) continue;
                    Logger.LogTrace(0, "Deleting Identifier {@identifier}", existingId);
                    Logger.LogDebug(0, "Deleting Identifier {identifier}", existingId.Id);
                    existingId.Deleted = DateTime.Now;
                    changed = true;
                }

                foreach (var identifier in identifiers)
                {
                    var marshalledValue = marshaller.MarshalledValue(identifier);

                    if (existing.Any(_ => _.Value == marshalledValue)) continue;


                    Logger.LogTrace(0, "Adding Identifier {@identifier} for Person{PersonId}", identifier, person.Id);
                    Logger.LogDebug(
                        0,
                        "Adding Identifier Type {identifier} for Person{PersonId}",
                        identifier.GetType(),
                        person.Id);

                    existing.Add(
                        stores.Get<PersonIdentifierEntity>().Add(
                            new PersonIdentifierEntity
                            {
                                Person = person,
                                TypeName = typeName,
                                Value = marshalledValue,
                                ValueType = marshaller.ValueType
                            }));
                    changed = true;
                }

                return changed;
            }
*/
        }
    }
}