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
        private IStore<PersonEntity> People => stores.Get<PersonEntity>();
        private readonly IIdentifierHandlerProvider handlerProvider;
        private readonly ITypeRegistry identifierRegistry;

        public IdentityRepository(
            ITypeRegistry<IPersonIdentifier> identifierRegistry, 
            IIdentifierHandlerProvider identifierHandlerProvider, 
            IStoreProvider stores)
        {
            this.stores = stores;
            this.identifierRegistry = identifierRegistry;
            handlerProvider = identifierHandlerProvider;
        }

        public PersonIdentity FindPersonBy(params IPersonIdentifier[] identifiers) => FindPersonBy(identifiers.AsEnumerable());
        
        public PersonIdentity FindPersonBy(IEnumerable<IPersonIdentifier> identifiers)
        {
            var filteredPeople = identifiers.Aggregate(People.AsQueryable(), FilterPeopleByIdentifier);
            return filteredPeople.SingleOrDefault();
        }

        private IQueryable<PersonEntity> FilterPeopleByIdentifier(
            IQueryable<PersonEntity> peopleEntities, 
            IPersonIdentifier identifier)
        {
            return GetHandler(identifier).Filter(peopleEntities, identifier, stores);
        }

        public IEnumerable<IPersonIdentifier> GetPersonIdentities(long personId)
        {
            var person = People.Get(personId);

            return identifierRegistry
                .Select(_ => _.Type)
                .Select(GetHandler)
                .SelectMany(handler => handler.Get(person, stores));

        }

        /// <inheritdoc />
        public PersonIdentity CreatePerson(IEnumerable<IPersonIdentifier> identifiers)
        {
            var person = People.Add(new PersonEntity());

            UpdatePerson(person, identifiers);
            
            return person;
            
        }

        /// <inheritdoc />
        public void UpdatePerson(PersonIdentity personIdentity, IEnumerable<IPersonIdentifier> identifiers)
        {
            Update(People.Get(personIdentity.Id), identifiers);
        }

        private void Update(PersonEntity person, IEnumerable<IPersonIdentifier> identifiers)
        {
            foreach (var identifierGroup in identifiers.GroupBy(_ => _.GetType()))
            {
                GetHandler(identifierGroup.Key).Update(person, identifierGroup.ToArray(), stores);
            }
        }

        private IPersonIdentifierHandler GetHandler(IPersonIdentifier identifier) =>
            GetHandler(identifier.GetType());
        private IPersonIdentifierHandler GetHandler(Type identifierType) =>
            handlerProvider.GetHandler(identifierType);
    }
}