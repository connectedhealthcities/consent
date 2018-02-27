using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;
using NeinLinq;
using NeinLinq.EntityFrameworkCore;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class MedwayNameIdentifierAdapter : 
        IIdentifierFilter<MedwayNameIdentifier>,
        IIdentifierUpdater<MedwayNameIdentifier>,
        IIdentifierRetriever<MedwayNameIdentifier>
    {
        /// <inheritdoc />
        public IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, MedwayNameIdentifier value, IStoreProvider stores) =>
            people.ToInjectable().Where(
                p => stores.Get<MedwayNameEntity>().WherePersonIs(p).Any(name => name.Matches(value)));

        /// <inheritdoc />
        public bool Update(PersonEntity person, MedwayNameIdentifier value, IStoreProvider stores)
        {
            var names = stores.Get<MedwayNameEntity>();
            var name = names.WherePersonIs(person).SingleOrDefault() ??
                       names.Add(new MedwayNameEntity {Person = stores.Get<PersonEntity>().Get(person.Id)});

            if (name.Matches(value)) return false;
            
            name.FirstName = value.FirstName;
            name.LastName = value.LastName;

            return true;
        }

        /// <inheritdoc />
        public IEnumerable<MedwayNameIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            var name = stores.Get<MedwayNameEntity>().WherePersonIs(person).SingleOrDefault();
            return name == null
                ? Enumerable.Empty<MedwayNameIdentifier>()
                : new[] {new MedwayNameIdentifier(name.FirstName, name.LastName)};
        }
    }

    public static class MedwayNameEntityQueries
    {
        private static readonly Expression<Func<MedwayNameEntity, PersonIdentity, bool>> ForPersonExpression =
            (name, person) => name.Person.Id == person.Id;

        private static readonly Func<MedwayNameEntity, PersonIdentity, bool> ForPersonCompiled = ForPersonExpression.Compile();

        private static readonly Expression<Func<MedwayNameEntity, MedwayNameIdentifier, bool>> MatchesExpression =
            (name, identifier) => name.FirstName == identifier.FirstName && name.LastName == identifier.LastName;

        private static readonly Func<MedwayNameEntity, MedwayNameIdentifier, bool> MatchesCompiled =
            MatchesExpression.Compile();

        [InjectLambda]
        public static bool ForPerson(this MedwayNameEntity name, PersonIdentity personIdentity) => ForPersonCompiled(name, personIdentity);

        public static Expression<Func<MedwayNameEntity, PersonIdentity, bool>> ForPerson() => ForPersonExpression;

        [InjectLambda]
        public static IQueryable<MedwayNameEntity>
            WherePersonIs(this IQueryable<MedwayNameEntity> names, PersonIdentity personIdentity) =>
            names.ToInjectable().Where(name => name.ForPerson(personIdentity));

        public static Expression<Func<IQueryable<MedwayNameEntity>, PersonIdentity, IQueryable<MedwayNameEntity>>>
            WherePersonIs() =>
            (names, person) => names.Where(name => name.ForPerson(person));

        [InjectLambda]
        public static bool Matches(this MedwayNameEntity name, MedwayNameIdentifier identifier) =>
            MatchesCompiled(name, identifier);

        public static Expression<Func<MedwayNameEntity, MedwayNameIdentifier, bool>> Matches() =>
            MatchesExpression;
    }

}