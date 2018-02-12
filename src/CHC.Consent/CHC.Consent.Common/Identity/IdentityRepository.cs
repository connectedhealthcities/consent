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

        public IdentityRepository(IStore<Person> people)
        {
            this.people = people;
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

        public Person AddPerson(Person person)
        {
            return people.Add(person);
        }
    }
}