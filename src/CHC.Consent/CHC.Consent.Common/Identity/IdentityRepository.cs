using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common.Infrastructure.Data;
using LinqKit;

namespace CHC.Consent.Common.Identity
{
    public class IdentityRepository
    {
        private readonly IStore<Person> people;
        private readonly IQueryable<IdentifierType> identifierTypes;

        public IdentityRepository(IStore<Person> people, IQueryable<IdentifierType> identifierTypes)
        {
            this.people = people;
            this.identifierTypes = identifierTypes;
        }

        public Person FindPersonBy(params Identifier[] identifiers) => FindPersonBy(identifiers.AsEnumerable());
        
        public Person FindPersonBy(IEnumerable<Identifier> identifiers)
        {


            var condition = identifiers
                .Select(GetExpression)
                .Aggregate(
                    PredicateBuilder.New<Person>(),
                    (current, predicate) => current.And(predicate)
                    );

            return people.AsExpandable().SingleOrDefault(condition);
        }

        private static Expression<Func<Person, bool>> GetExpression(Identifier identifier)
        {
            return identifier.GetMatchExpression();
        }

        public IdentifierType FindIdentifierType(string externalId)
        {
            return identifierTypes.FirstOrDefault(_ => _.ExternalId == externalId);
        }

        public Person AddPerson(Person person)
        {
            return people.Add(person);
        }
    }
}