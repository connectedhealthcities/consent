
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
            for (var i = 0; i < 5; i++)
            {
                saveContext.Add(new PersonEntity {Sex = Random.Enum<Sex>()});
            }

            var person = saveContext.Add(new PersonEntity {Sex = Sex.Male, NhsNumber = "FIND ME"});

            saveContext.SaveChanges();

            var foundpeople = new SexIdentifierAdapter().Filter(
                readContext.People,
                new SexIdentifier(Sex.Male), 
                new ContextWrapper(readContext)).ToArray();

            var found = Assert.Single(foundpeople, _ => _.NhsNumber == "FIND ME");
            Assert.NotNull(found);
            Assert.Equal(Sex.Male, found.Sex);
        }

        [Fact]
        public void CanSetPersonSex()
        {
            var person = saveContext.Add(new PersonEntity {Sex = default(Sex?)}).Entity;
            saveContext.SaveChanges();

            new SexIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new SexIdentifier(Sex.Female), 
                new ContextWrapper(Context)
            );
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal(Sex.Female, saved.Sex);
        }

        [Fact()]
        public void CanSetSexForNewPerson()
        {
            var person = new PersonEntity();

            new SexIdentifierAdapter().Update(
                person,
                new SexIdentifier(Sex.Female),
                new ContextWrapper(Context)
            );

            Context.Add(person);
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal(Sex.Female, saved.Sex);
        }

        [Fact]
        public void CannotUpdateSex()
        {
            var person = saveContext.Add(new PersonEntity {Sex = Sex.Male}).Entity;
            saveContext.SaveChanges();
            

            Assert.Throws<InvalidOperationException>(() => new SexIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new SexIdentifier(Sex.Female), 
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