using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Security;
using NeinLinq.EntityFrameworkCore;

namespace CHC.Consent.EFCore
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly IStoreProvider stores;
        private IStore<PersonEntity> People => stores.Get<PersonEntity>();
        private readonly PersonIdentifierHandlerProvider handlers;

        public IdentityRepository(
            ITypeRegistry<IPersonIdentifier> identifierRegistry, 
            IIdentifierHandlerProvider identifierHandlerProvider, 
            IStoreProvider stores)
        {
            this.stores = stores;

            handlers = new PersonIdentifierHandlerProvider(identifierHandlerProvider, identifierRegistry);
        }

        public PersonIdentity FindPersonBy(params IPersonIdentifier[] identifiers) => 
            FindPersonBy(identifiers.AsEnumerable());
        
        public PersonIdentity FindPersonBy(IEnumerable<IPersonIdentifier> identifiers)
        {
            var filteredPeople = identifiers.Aggregate(People.AsQueryable(), FilterPeopleByIdentifier);
            return filteredPeople.SingleOrDefault();
        }

        private IQueryable<PersonEntity> FilterPeopleByIdentifier(
            IQueryable<PersonEntity> peopleEntities, 
            IPersonIdentifier identifier)
        {
            return handlers.GetPersistanceHandler(identifier).Filter(peopleEntities, identifier, stores);
        }

        public IEnumerable<IPersonIdentifier> GetPersonIdentifiers(long personId)
        {
            var person = People.Get(personId);

            return handlers.AllPersistanceHandlers()
                .SelectMany(handler => handler.GetIdentifiers(person, stores));

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

        /// <inheritdoc />
        public IDictionary<PersonIdentity, IDictionary<string, IEnumerable<IPersonIdentifier>>>
            GetPeopleWithIdentifiers(
                IEnumerable<PersonIdentity> personIds,
                IEnumerable<string> identifierNames,
                IUserProvider user)
        {
            var userName = user.UserName;
            var roles = user.Roles.ToArray();

            var peopleIdValues = personIds.Select(_ => _.Id).ToArray();
            var people = People.ToInjectable()
                .Where(p => p.GrantsPermission(user, "read"))
                .Where(p => peopleIdValues.Contains(p.Id))
                .Distinct()
                .ToList();

            var identifiers = new Dictionary<PersonIdentity, IDictionary<string, IEnumerable<IPersonIdentifier>>>();
            var namesAndHandlers = identifierNames.Distinct().Select(_ => (_, handlers.GetPersistanceHandler(_))).ToArray(); 
            
            foreach (var personEntity in people)
            {
                var personIdentifiers = new Dictionary<string, IEnumerable<IPersonIdentifier>>();
                foreach (var (name, handler) in namesAndHandlers)
                {
                    personIdentifiers[name] = handler.GetIdentifiers(personEntity, stores);
                }

                identifiers[personEntity] = personIdentifiers;
            }

            return identifiers;
        }

        private void Update(PersonEntity person, IEnumerable<IPersonIdentifier> identifiers)
        {
            foreach (var identifierGroup in identifiers.GroupBy(_ => _.GetType()))
            {
                handlers.GetPersistanceHandler(identifierGroup.Key).Update(person, identifierGroup.ToArray(), stores);
            }
        }
    }
}