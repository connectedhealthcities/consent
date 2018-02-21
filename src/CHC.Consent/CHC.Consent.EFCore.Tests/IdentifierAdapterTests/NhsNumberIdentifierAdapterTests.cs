
using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.IdentifierAdapters;
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
            for (var i = 0; i < 5; i++)
            {
                saveContext.Add(new PersonEntity {NhsNumber = Random.String()});
            }

            var person = saveContext.Add(new PersonEntity {NhsNumber = "FIND ME"});

            saveContext.SaveChanges();

            var foundpeople = new NhsNumberIdentifierAdapter().Filter(
                readContext.People,
                new NhsNumberIdentifier("FIND ME"),
                new ContextWrapper(readContext)).ToArray();

            var found = Assert.Single(foundpeople);
            Assert.NotNull(found);
            Assert.Equal("FIND ME", found.NhsNumber);
        }

        [Fact]
        public void CanSetPersonNhsNumber()
        {
            var person = saveContext.Add(new PersonEntity {NhsNumber = default(string)}).Entity;
            saveContext.SaveChanges();

            new NhsNumberIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new NhsNumberIdentifier("NEW NHS NUMBER"),
                new ContextWrapper(Context)
            );
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal("NEW NHS NUMBER", saved.NhsNumber);
        }

        [Fact]
        public void CanSetNhsNumberForNewPerson()
        {
            var person = new PersonEntity();

            new NhsNumberIdentifierAdapter().Update(
                person,
                new NhsNumberIdentifier("NEW NHS NUMBER"),
                new ContextWrapper(Context)
            );

            Context.Add(person);
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal("NEW NHS NUMBER", saved.NhsNumber);
        }

        [Fact]
        public void CannotUpdateChangeNhsNumber()
        {
            var person = saveContext.Add(new PersonEntity {NhsNumber = "EXISTING NHS NUMBER"}).Entity;
            saveContext.SaveChanges();
            

            Assert.Throws<InvalidOperationException>(() => new NhsNumberIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new NhsNumberIdentifier("NEW NHS NUMBER"),
                new ContextWrapper(Context)
            ));
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