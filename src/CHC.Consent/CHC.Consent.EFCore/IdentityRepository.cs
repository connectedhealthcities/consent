using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EFCore
{
    public class IdentityRepository : IIdentityRepository
    {
        private readonly ConsentContext context;

        private readonly PersonIdentifierXmlMarshallers marshallers;

        public IdentityRepository(
            IdentifierDefinitionRegistry registry,
            ConsentContext context)
        {
            this.context = context;
            marshallers = new PersonIdentifierXmlMarshallers(registry);
        }

        private DbSet<PersonIdentifierEntity> IdentifierEntities => context.PersonIdentifiers;
        private DbSet<PersonEntity> People => context.People;
        private DbSet<ConsentEntity> Consent => context.Set<ConsentEntity>();
        private DbSet<AuthorityEntity> Authorities => context.Set<AuthorityEntity>();
        private DbSet<AgencyEntity> Agencies => context.Set<AgencyEntity>();

        public PersonIdentity FindPersonBy(IPersonSpecification specification)
        {
            var predicate = BuildExpressionFromSpecification(specification);
            return People.AsNoTracking().SingleOrDefault(predicate);
        }

        public IEnumerable<PersonIdentifier> GetPersonIdentifiers(long personId)
        {
            var identifierEntities = IdentifierEntities.AsNoTracking()
                .Where(_ => _.Person.Id == personId && _.Deleted == null).AsEnumerable();
            return marshallers.MarshallFromXml(identifierEntities);
        }

        /// <inheritdoc />
        public PersonIdentity CreatePerson(IEnumerable<PersonIdentifier> identifiers, Authority authority)
        {
            var person = new PersonEntity();

            People.Add(person);
            UpdatePerson(person, identifiers, authority);
            context.SaveChanges(acceptAllChangesOnSuccess: true);

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

        public Agency GetAgency(string systemName)
            =>
                Agencies
                    .Include(_ => _.Fields)
                    .ThenInclude(_ => _.Identifier)
                    .SingleOrDefault(_ => _.SystemName == systemName);

        public string GetPersonAgencyId(PersonIdentity personId, AgencyIdentity agencyId)
        {
            var existing = context.Set<PersonAgencyId>()
                .SingleOrDefault(_ => _.PersonId == personId.Id && _.AgencyId == agencyId.Id);

            if (existing != null) return existing.SpecificId;

            var id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).TrimEnd('=');
            context.Set<PersonAgencyId>()
                .Add(new PersonAgencyId {PersonId = personId.Id, AgencyId = agencyId.Id, SpecificId = id});
            context.SaveChanges();

            return id;
        }

        private Expression<Func<PersonEntity, bool>> BuildExpressionFromSpecification(
            IPersonSpecification specification)
        {
            return new PersonPredicateBuilder(context, MarshallValue).BuildPredicate(specification);
        }

        private string MarshallValue(PersonIdentifier identifier)
        {
            return marshallers.MarshallToXml(identifier).ToString(SaveOptions.DisableFormatting);
        }

        private IDictionary<PersonIdentity, IEnumerable<PersonIdentifier>> PeopleWithIdentifiers(
            IEnumerable<string> identifierNames,
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

        private void Update(PersonEntity person, IEnumerable<PersonIdentifier> identifiers, Authority authority)
        {
            var authorityEntity = Authorities.Find((long) authority.Id);
            var storedIdentifiers = ExistingIdentifierEntities(person).ToList();
            foreach (var identifierGroup in identifiers.GroupBy(_ => _.Definition))
            {
                var definition = identifierGroup.Key;

                foreach (var existingId in storedIdentifiers.Where(_ => _.TypeName == definition.SystemName))
                {
                    if (identifierGroup.All(
                        _ => existingId.Value != MarshallValue(_) &&
                             existingId.Authority.Priority >= authority.Priority))
                        existingId.Deleted = DateTime.Now;
                }

                foreach (var identifier in identifierGroup)
                {
                    var marshalledValue = MarshallValue(identifier);

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

        private class PersonPredicateBuilder : IPersonSpecificationVisitor
        {
            private readonly ConsentContext context;


            /// <inheritdoc />
            public PersonPredicateBuilder(
                ConsentContext context,
                Func<PersonIdentifier, string> marshallValue
            )
            {
                MarshallValue = marshallValue;
                this.context = context;
            }

            private Func<PersonIdentifier, string> MarshallValue { get; }

            public ExpressionStarter<PersonEntity> Predicate { get; private set; } =
                PredicateBuilder.New<PersonEntity>();

            /// <inheritdoc />
            public void Visit(PersonIdentifierSpecification specification)
            {
                var identifier = specification.PersonIdentifier;
                var xmlValue = MarshallValue(identifier);

                Add(
                    p => context.Set<PersonIdentifierEntity>().Any(
                        _ => _.Person == p
                             && _.TypeName == identifier.Definition.SystemName
                             && _.Deleted == null
                             && _.Value == xmlValue));
            }

            /// <inheritdoc />
            public void Visit(AgencyIdentifierPersonSpecification specification)
            {
                Add(
                    p =>
                        (from id in context.Set<PersonAgencyId>()
                            join agency in context.Set<AgencyEntity>() on id.AgencyId equals agency.Id
                            where id.PersonId == p.Id
                                  && agency.SystemName == specification.AgencyName
                                  && id.SpecificId == specification.PersonAgencyId
                            select id.Id).Any()
                );
            }

            /// <inheritdoc />
            public void Visit(ConsentedPersonSpecification specification)
            {
                Add(
                    p =>
                        context.Set<ConsentEntity>().Any(
                            c => c.StudySubject.Person == p
                                 && c.StudySubject.Study.Id == specification.StudyId.Id
                                 && c.DateWithdrawn == null
                        )
                );
            }

            private void Add(Expression<Func<PersonEntity, bool>> expression)
            {
                Predicate = Predicate.And(expression);
            }

            public Expression<Func<PersonEntity, bool>> BuildPredicate(IPersonSpecification specification)
            {
                specification.Accept(this);
                return Predicate;
            }
        }
    }
}