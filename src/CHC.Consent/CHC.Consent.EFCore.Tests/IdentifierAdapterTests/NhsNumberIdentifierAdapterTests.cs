
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
    
    public class NhsNumberIdentifierAdapterTests : DbTests
    {
        private readonly ConsentContext saveContext;
        private readonly ConsentContext readContext;

        /// <inheritdoc />
        public NhsNumberIdentifierAdapterTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            saveContext = CreateNewContextInSameTransaction();
            readContext = CreateNewContextInSameTransaction();
        }

        [Fact]
        public void CorrectlyFindsPersonByNhsNumber()
        {
            5.Times(() => { AddPersonWithNhsNumber(Random.String()); });

            var searchForMe = AddPersonWithNhsNumber("FIND ME");

            saveContext.SaveChanges();

            var foundpeople = new NhsNumberIdentifierAdapter().Filter(
                readContext.People,
                new NhsNumberIdentifier("FIND ME"),
                new ContextStoreProvider(readContext)).ToArray();

            var found = Assert.Single(foundpeople);
            Assert.NotNull(found);
            Assert.Equal(searchForMe.Id, found.Id);
        }

        [Fact]
        public void CanSetPersonNhsNumber()
        {
            var person = saveContext.Add(new PersonEntity {NhsNumber = default}).Entity;
            saveContext.SaveChanges();

            new NhsNumberIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new NhsNumberIdentifier("NEW NHS NUMBER"),
                new ContextStoreProvider(Context)
            );
            Context.SaveChanges();

            AssertHasActiveNhsNumber(person, "NEW NHS NUMBER");
        }

        [Fact]
        public void CanSetNhsNumberForNewPerson()
        {
            var person = new PersonEntity();

            new NhsNumberIdentifierAdapter().Update(
                person,
                new NhsNumberIdentifier("NEW NHS NUMBER"),
                new ContextStoreProvider(Context)
            );

            Context.Add(person);
            Context.SaveChanges();

            AssertHasActiveNhsNumber(person, "NEW NHS NUMBER");
        }

        [Fact]
        public void UpdatingNhsNumberAddsNewRecord()
        {
            var person = AddPersonWithNhsNumber("EXISTING NHS NUMBER");
            saveContext.SaveChanges();


            var updateResult = new NhsNumberIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new NhsNumberIdentifier("NEW NHS NUMBER"),
                new ContextStoreProvider(Context));
            

            Context.SaveChanges();

            Assert.True(updateResult);
            var saved = GetStoreIdentifiersFor(person, readContext).ToArray();

            Assert.Collection(
                saved,
                _ =>
                {
                    Assert.Equal("EXISTING NHS NUMBER", _.Value);
                    Assert.NotNull(_.Deleted);
                },
                _ =>
                {
                    Assert.Equal("NEW NHS NUMBER", _.Value);
                    Assert.Null(_.Deleted);
                });

        }

        public IEnumerable<IdentifierEntity> GetStoreIdentifiersFor(PersonEntity person, ConsentContext context) => 
            context
                .Set<IdentifierEntity>()
                .Include(_ => _.Person)
                .Where(_ => _.Person.Id == person.Id && _.TypeName == NhsNumberIdentifier.TypeName)
                .OrderBy(_ => _.Created);

        private void AssertHasActiveNhsNumber(PersonEntity person, string nhsNumber)
        {
            var saved = GetStoreIdentifiersFor(person, readContext).SingleOrDefault();
            Assert.Equal(nhsNumber, saved.Value);
            Assert.Null(saved.Deleted);
        }

        private PersonEntity AddPersonWithNhsNumber(string nhsNumber)
        {
            var person = saveContext.Add(new PersonEntity()).Entity;
            saveContext.Add(
                new IdentifierEntity
                {
                    Person = person,
                    TypeName = NhsNumberIdentifier.TypeName,
                    Value = nhsNumber,
                    ValueType = "string"
                });
            return person;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            saveContext.Dispose();
            readContext.Dispose();
            base.Dispose();
        }
    }
}