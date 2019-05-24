using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.EFCore.Security;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using NeinLinq;

namespace CHC.Consent.EFCore
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly ConsentContext context;
        private DbSet<PersonIdentifierEntity> IdentifierEntities => context.PersonIdentifiers;
        private DbSet<PersonEntity> People => context.People;
        private DbSet<ConsentEntity> Consent => context.Set<ConsentEntity>();
        private DbSet<AuthorityEntity> Authorities => context.Set<AuthorityEntity>();
        private DbSet<AgencyEntity> Agencies => context.Set<AgencyEntity>();
        
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
        public PersonIdentity CreatePerson(IEnumerable<PersonIdentifier> identifiers, Authority authority)
        {
            var person = new PersonEntity();
            
            People.Add(person);
            UpdatePerson(person, identifiers, authority);
            context.SaveChanges(acceptAllChangesOnSuccess:true);
            
            return person;
        }

        /// <inheritdoc />
        public void UpdatePerson(
            PersonIdentity personIdentity, IEnumerable<PersonIdentifier> identifiers, Authority authority)
        {
            Update(People.Find(personIdentity.Id), identifiers, authority);
        }

        /// <inheritdoc />
        public IDictionary<PersonIdentity, IEnumerable<PersonIdentifier>>
            GetPeopleWithIdentifiers(
                IEnumerable<PersonIdentity> personIds,
                IEnumerable<string> identifierNames,
                IUserProvider user)
        {
            identifierNames = identifierNames.Distinct().ToArray();
            return PeopleWithIdentifiers(identifierNames, new HasIdCriteria(personIds));
        }

        private IDictionary<PersonIdentity, IEnumerable<PersonIdentifier>> PeopleWithIdentifiers(IEnumerable<string> identifierNames,
            params ICriteria<PersonEntity>[] search)
        {
            return
                People.AsNoTracking().Search(context, search)
                    .Select(
                        _ => new {_.Id, Identifiers = _.Identifiers.Where(i => identifierNames.Contains(i.TypeName))})
                    .ToDictionary(
                        _ => new PersonIdentity(_.Id),
                        _ => _.Identifiers.Select(marshallers.MarshallFromXml).ToArray().AsEnumerable());
        }


        public IDictionary<PersonIdentity, IEnumerable<PersonIdentifier>>
            GetPeopleWithIdentifiers(
                IEnumerable<PersonIdentity> personIds,
                IEnumerable<string> identifierNames,
                IUserProvider user,
                IEnumerable<IdentifierSearch> search
            )
        {
            return PeopleWithIdentifiers(
                identifierNames,
                new HasIdentifiersCriteria(search),
                new HasIdCriteria(personIds.Select(_ => _.Id)));
        }

        /// <inheritdoc />
        public Authority GetAuthority(string systemName)
        {
            return Authorities.SingleOrDefault(_ => _.SystemName == systemName)?.ToAuthority();
        }

        private void Update(PersonEntity person, IEnumerable<PersonIdentifier> identifiers, Authority authority)
        {
            var authorityEntity = Authorities.Find((long)authority.Id);
            var storedIdentifiers = ExistingIdentifierEntities(person).ToList();
            foreach (var identifierGroup in identifiers.GroupBy(_ => _.Definition))
            {
                var definition = identifierGroup.Key;

                foreach (var existingId in storedIdentifiers.Where(_ => _.TypeName == definition.SystemName))
                {
                    if (identifierGroup.All(_ => existingId.Value != MarshalledValue(_) && existingId.Authority.Priority >= authority.Priority))
                        existingId.Deleted = DateTime.Now;
                }

                foreach (var identifier in identifierGroup)
                {
                    var marshalledValue = MarshalledValue(identifier);

                    if (storedIdentifiers.Any(_ => _.Value == marshalledValue)) continue;

                    storedIdentifiers.Add(
                        IdentifierEntities.Add(
                            new PersonIdentifierEntity
                            {
                                Person = person,
                                TypeName = definition.SystemName,
                                Value = marshalledValue,
                                ValueType = definition.Type.SystemName,
                                Authority = authorityEntity
                            }).Entity);
                }
            }
        }

        private IEnumerable<PersonIdentifierEntity> ExistingIdentifierEntities(PersonEntity person)
        {
            return IdentifierEntities.Where(_ => _.Person == person && _.Deleted == null).Include(_ => _.Authority);
        }

        public Agency GetAgency(string systemName)
            =>
                Agencies
                    .Include(_ => _.Fields)
                    .ThenInclude(_ => _.Identifier)
                    .SingleOrDefault(_ => _.SystemName == systemName);
    }
}
