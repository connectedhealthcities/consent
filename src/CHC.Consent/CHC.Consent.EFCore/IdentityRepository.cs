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
                    (filteredPeople, identifier) =>
                        handlerProvider.GetFilter(identifier).Filter(filteredPeople, identifier, stores))
                .SingleOrDefault();
        }

        public IEnumerable<IPersonIdentifier> GetPersonIdentities(long personId)
        {
            var person = people.Get(personId);

            return identifierRegistry.Select(_ => _.Type)
                .SelectMany(identifierType => handlerProvider.GetRetriever(identifierType).Get(person, stores));

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
                handlerProvider.GetUpdater(identifier.GetType()).Update(person, identifier, stores);
            }
        }
    }
}