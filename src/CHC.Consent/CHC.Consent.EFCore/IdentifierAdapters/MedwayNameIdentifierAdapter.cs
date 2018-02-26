using System;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.EFCore.Entities;
using NeinLinq;
using NeinLinq.EntityFrameworkCore;

namespace CHC.Consent.EFCore.IdentifierAdapters
{
    public class MedwayNameIdentifierAdapter : 
        IIdentifierFilter<MedwayNameIdentifier>,
        IIdentifierUpdater<MedwayNameIdentifier>
    {
        /// <inheritdoc />
        public IQueryable<TPerson> Filter<TPerson>(
            IQueryable<TPerson> people, MedwayNameIdentifier value, IStoreProvider stores) where TPerson : Person =>
            people.ToInjectable().Where(
                p => stores.Get<MedwayNameEntity>().WherePersonIs(p).Any(name => name.Matches(value)));

        /// <inheritdoc />
        public bool Update(Person person, MedwayNameIdentifier value, IStoreProvider stores)
        {
            var names = stores.Get<MedwayNameEntity>();
            var name = names.WherePersonIs(person).SingleOrDefault() ??
                       names.Add(new MedwayNameEntity {Person = stores.Get<PersonEntity>().Get(person.Id)});

            if (name.Matches(value)) return false;
            
            name.FirstName = value.FirstName;
            name.LastName = value.LastName;

            return true;
        }
    }

    public static class MedwayNameEntityQueries
    {
        private static readonly Expression<Func<MedwayNameEntity, Person, bool>> ForPersonExpression =
            (name, person) => name.Person.Id == person.Id;

        private static readonly Func<MedwayNameEntity, Person, bool> ForPersonCompiled = ForPersonExpression.Compile();

        private static readonly Expression<Func<MedwayNameEntity, MedwayNameIdentifier, bool>> MatchesExpression =
            (name, identifier) => name.FirstName == identifier.FirstName && name.LastName == identifier.LastName;

        private static readonly Func<MedwayNameEntity, MedwayNameIdentifier, bool> MatchesCompiled =
            MatchesExpression.Compile();

        [InjectLambda]
        public static bool ForPerson(this MedwayNameEntity name, Person person) =>
            ForPersonCompiled(name, person);

        public static Expression<Func<MedwayNameEntity, Person, bool>> ForPerson() =>
            ForPersonExpression;

        [InjectLambda]
        public static IQueryable<MedwayNameEntity>
            WherePersonIs(this IQueryable<MedwayNameEntity> names, Person person) =>
            names.ToInjectable().Where(name => name.ForPerson(person));

        public static Expression<Func<IQueryable<MedwayNameEntity>, Person, IQueryable<MedwayNameEntity>>>
            WherePersonIs() =>
            (names, person) => names.Where(name => name.ForPerson(person));

        [InjectLambda]
        public static bool Matches(this MedwayNameEntity name, MedwayNameIdentifier identifier) =>
            MatchesCompiled(name, identifier);

        public static Expression<Func<MedwayNameEntity, MedwayNameIdentifier, bool>> Matches() =>
            MatchesExpression;
    }

}