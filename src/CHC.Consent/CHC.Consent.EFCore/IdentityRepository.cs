using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly IStoreProvider stores;
        private readonly IStore<PersonEntity> people;
        private readonly IIdentifierHandlerProvider handlerProvider;
        private readonly ITypeRegistry identifierRegistry;

        public IdentityRepository(
            IStore<PersonEntity> people, 
            ITypeRegistry<IPersonIdentifier> identifierRegistry, 
            IIdentifierHandlerProvider identifierHandlerProvider, 
            IStoreProvider stores)
        {
            this.stores = stores;
            this.people = people;
            this.identifierRegistry = identifierRegistry;
            handlerProvider = identifierHandlerProvider;
        }

        public PersonIdentity FindPersonBy(params IPersonIdentifier[] identifiers) => FindPersonBy(identifiers.AsEnumerable());
        
        public PersonIdentity FindPersonBy(IEnumerable<IPersonIdentifier> identifiers)
        {
            return identifiers.Aggregate(
                    (IQueryable<PersonEntity>) people,
                    FilterPeopleByIdentifier)
                .SingleOrDefault();
        }

        private IQueryable<PersonEntity> FilterPeopleByIdentifier(
            IQueryable<PersonEntity> peopleEntities, IPersonIdentifier identifier)
        {
            return GetHandler(identifier).Filter(peopleEntities, identifier, stores);
        }

        public IEnumerable<IPersonIdentifier> GetPersonIdentities(long personId)
        {
            var person = people.Get(personId);

            return identifierRegistry
                .Select(_ => _.Type)
                .Select(GetHandler)
                .SelectMany(handler => handler.Get(person, stores));

        }

        /// <inheritdoc />
        public PersonIdentity CreatePerson(IEnumerable<IPersonIdentifier> identifiers)
        {
            var person = people.Add(new PersonEntity());

            UpdatePerson(person, identifiers);
            
            return person;
            
        }

        /// <inheritdoc />
        public void UpdatePerson(PersonIdentity personIdentity, IEnumerable<IPersonIdentifier> identifiers)
        {
            Update(people.Get(personIdentity.Id), identifiers);
        }

        private void Update(PersonEntity person, IEnumerable<IPersonIdentifier> identifiers)
        {
            foreach (var identifier in identifiers)
            {
                GetHandler(identifier).Update(person, identifier, stores);
            }
        }

        private IPersonIdentifierHandler GetHandler(IPersonIdentifier identifier) =>
            GetHandler(identifier.GetType());
        private IPersonIdentifierHandler GetHandler(Type identifierType) =>
            handlerProvider.GetHandler(identifierType);
    }
}