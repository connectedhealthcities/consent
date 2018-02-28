
using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.IdentifierAdapters;
using CHC.Consent.Testing.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.EFCore.Tests.IdentifierAdapterTests
{
    public class SexIdentifierAdapterTests : DbTests
    {
        private readonly ConsentContext saveContext;
        private readonly ConsentContext readContext;

        /// <inheritdoc />
        public SexIdentifierAdapterTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            saveContext = CreateNewContextInSameTransaction();
            readContext = CreateNewContextInSameTransaction();
        }

        [Fact]
        public void CorrectlyFindsPersonBySex()
        {
            5.Times(() => AddAPersonWithSex(Random.Item(Sex.Female, Sex.Unknown, (Sex?)null)));
            var person = AddAPersonWithSex(Sex.Male);
            saveContext.SaveChanges();

            var foundpeople = new SexIdentifierAdapter().Filter(
                readContext.People,
                new SexIdentifier(Sex.Male), 
                new ContextStoreProvider(readContext)).ToArray();

            var found = Assert.Single(foundpeople);
            Assert.NotNull(found);
            Assert.Equal(person.Id, found.Id);
        }

        private PersonEntity AddAPersonWithSex(Sex? sex)
        {
            var personEntity = saveContext.Add(new PersonEntity {}).Entity;
            saveContext.Add(
                new IdentifierEntity
                {
                    Person = personEntity,
                    TypeName = SexIdentifier.TypeName,
                    Value = sex?.ToString(),
                    ValueType = typeof(Sex?).AssemblyQualifiedName
                });
            return personEntity;
        }

        [Fact]
        public void CanSetPersonSex()
        {
            var person = AddAPersonWithSex(Sex.Male);
            saveContext.SaveChanges();

            new SexIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new SexIdentifier(Sex.Female), 
                new ContextStoreProvider(Context)
            );
            Context.SaveChanges();

            AssertHasActiveSex(person, Sex.Female);
        }

        [Fact()]
        public void CanSetSexForNewPerson()
        {
            var person = Context.Add(new PersonEntity()).Entity;

            new SexIdentifierAdapter().Update(
                person,
                new SexIdentifier(Sex.Female),
                new ContextStoreProvider(Context)
            );

            Context.Add(person);
            Context.SaveChanges();

            AssertHasActiveSex(person, Sex.Female);
        }

        [Fact]
        public void StoresPreviousSexWhenUpdating()
        {
            var person = AddAPersonWithSex(Sex.Male);
            saveContext.SaveChanges();
            

            var updateResult = new SexIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new SexIdentifier(Sex.Female), 
                new ContextStoreProvider(Context)
            );
            
            Context.SaveChanges();

            Assert.True(updateResult);
            var saved = GetStoreIdentifiersFor(person, readContext).ToArray();

            Assert.Collection(
                saved,
                _ =>
                {
                    Assert.Equal(Sex.Male.ToString(), _.Value);
                    Assert.NotNull(_.Deleted);
                },
                _ =>
                {
                    Assert.Equal(Sex.Female.ToString(), _.Value);
                    Assert.Null(_.Deleted);
                });
            
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            saveContext.Dispose();
            readContext.Dispose();
            base.Dispose();
        }
        
        public IEnumerable<IdentifierEntity> GetStoreIdentifiersFor(PersonEntity person, ConsentContext context) => 
            context
                .Set<IdentifierEntity>()
                .Include(_ => _.Person)
                .Where(_ => _.Person.Id == person.Id && _.TypeName == SexIdentifier.TypeName)
                .OrderBy(_ => _.Created);

        private void AssertHasActiveSex(PersonEntity person, Sex sex)
        {
            var saved = GetStoreIdentifiersFor(person, readContext).SingleOrDefault(_ => _.Deleted == null);
            Assert.NotNull(saved);
            Assert.Equal(sex.ToString(), saved.Value);
            Assert.Null(saved.Deleted);
        }
    }
}