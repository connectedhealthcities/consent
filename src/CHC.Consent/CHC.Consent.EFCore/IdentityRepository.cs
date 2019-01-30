using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Entities;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EFCore
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly ConsentContext context;
        private DbSet<PersonIdentifierEntity> IdentifierEntities => context.PersonIdentifiers;
        private DbSet<PersonEntity> People => context.People;
        
        private readonly PersonIdentifierXmlMarshallers marshallers;

        public IdentityRepository(
            IdentifierDefinitionRegistry registry, 
            ConsentContext context)
        {
            this.context = context;
            marshallers = new PersonIdentifierXmlMarshallers(registry);
        }

        /// <summary>
        /// Convenience Method, mostly used for testing
        /// </summary>
        public PersonIdentity FindPersonBy(params PersonIdentifier[] identifiers) => 
            FindPersonBy(identifiers.AsEnumerable());
        
        public PersonIdentity FindPersonBy(IEnumerable<PersonIdentifier> identifiers)
        {
            var predicate = identifiers.Aggregate(
                PredicateBuilder.New<PersonEntity>(),
                (t, i) =>
                {
                    var xmlValue = MarshalledValue(i);
                    return t.Or(
                        p => IdentifierEntities.Any(
                                 _ => _.Person == p
                                      && _.TypeName == i.Definition.SystemName
                                      && _.Deleted == null
                                      && _.Value == xmlValue));
                }
                );
            return People.AsNoTracking().SingleOrDefault(predicate);
        }

        private string MarshalledValue(PersonIdentifier identifier)
        {
            return marshallers.MarshallToXml(identifier).ToString(SaveOptions.DisableFormatting);
        }

        public IEnumerable<PersonIdentifier> GetPersonIdentifiers(long personId)
        {
            var identifierEntities = IdentifierEntities.AsNoTracking().Where(_ => _.Person.Id == personId && _.Deleted == null).AsEnumerable();
            return marshallers.MarshallFromXml(identifierEntities);
        }

        /// <inheritdoc />
        public PersonIdentity CreatePerson(IEnumerable<PersonIdentifier> identifiers)
        {
            var person = new PersonEntity();
            
            People.Add(person);
            UpdatePerson(person, identifiers);
            context.SaveChanges(acceptAllChangesOnSuccess:true);
            
            return person;
        }

        /// <inheritdoc />
        public void UpdatePerson(PersonIdentity personIdentity, IEnumerable<PersonIdentifier> identifiers)
        {
            Update(People.Find(personIdentity.Id), identifiers);
        }

        /// <inheritdoc />
        public IDictionary<PersonIdentity, IDictionary<string, IEnumerable<PersonIdentifier>>>
            GetPeopleWithIdentifiers(
                IEnumerable<PersonIdentity> personIds,
                IEnumerable<string> identifierNames,
                IUserProvider user)
        {
            var peopleIdValues = personIds.Select(_ => _.Id).ToArray();
            identifierNames = identifierNames.Distinct().ToArray();


            return IdentifierEntities.AsNoTracking()
                .Where(_ => peopleIdValues.Contains(_.Person.Id) && identifierNames.Contains(_.TypeName) && _.Deleted == null)
                .Select(_ => new {Identity = new PersonIdentity(_.Person.Id), Identifier = marshallers.MarshallFromXml(_)})
                .GroupBy(_ => _.Identity, _ => _.Identifier)
                .ToDictionary(
                    _ => _.Key,
                    _ =>
                        (IDictionary<string, IEnumerable<PersonIdentifier>>)
                        _.GroupBy(i => i.Definition.SystemName)
                            .ToDictionary(i => i.Key, i => i.ToArray().AsEnumerable()));
        }

        private void Update(PersonEntity person, IEnumerable<PersonIdentifier> identifiers)
        {
            var existing = ExistingIdentifierEntities(person).ToList();
            foreach (var identifierGroup in identifiers.GroupBy(_ => _.Definition))
            {
                var definition = identifierGroup.Key;

                foreach (var existingId in existing.Where(_ => _.TypeName == definition.SystemName))
                {
                    if (identifierGroup.All(_ => existingId.Value != MarshalledValue(_)))
                        existingId.Deleted = DateTime.Now;
                }

                foreach (var identifier in identifierGroup)
                {
                    var marshalledValue = MarshalledValue(identifier);

                    if (existing.Any(_ => _.Value == marshalledValue)) continue;

                    existing.Add(
                        IdentifierEntities.Add(
                            new PersonIdentifierEntity
                            {
                                Person = person,
                                TypeName = definition.SystemName,
                                Value = marshalledValue,
                                ValueType = definition.Type.SystemName
                            }).Entity);
                }
            }
        }

        private IEnumerable<PersonIdentifierEntity> ExistingIdentifierEntities(PersonEntity person)
        {
            return IdentifierEntities.Where(_ => _.Person == person && _.Deleted == null);
        }
    }
}