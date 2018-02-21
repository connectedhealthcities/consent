
using System;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.IdentifierAdapters;
using CHC.Consent.Testing.Utils;
using Xunit;
using Xunit.Abstractions;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.EFCore.Tests.IdentifierAdapterTests
{
    
    public class DateOfBirthIdentifierAdapterTests : DbTests
    {
        private readonly ConsentContext saveContext;
        private readonly ConsentContext readContext;

        /// <inheritdoc />
        public DateOfBirthIdentifierAdapterTests (ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            saveContext = CreateNewContextInSameTransaction();
            readContext = CreateNewContextInSameTransaction();
        }

        [Fact]
        public void CorrectlyFindsPersonByDateOfBirth()
        {
            for (var i = 0; i < 5; i++)
            {
                saveContext.Add(new PersonEntity {DateOfBirth= Random.Date()});
            }

            var findValue = 7.June(2041);
            var person = saveContext.Add(new PersonEntity { DateOfBirth = findValue});

            saveContext.SaveChanges();

            var found = new DateOfBirthIdentifierAdapter().Filter(
                readContext.People,
                new DateOfBirthIdentifier{ DateOfBirth= findValue}, 
                new ContextWrapper(readContext));

            Assert.Single(found);
        }

        [Fact]
        public void CanSetPersonNhsNumber()
        {
            var person = saveContext.Add(new PersonEntity {DateOfBirth = default(DateTime?)}).Entity;
            saveContext.SaveChanges();

            var newDateOfBirth = 25.April(1876);
            
            new DateOfBirthIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new DateOfBirthIdentifier{ DateOfBirth = newDateOfBirth}, 
                new ContextWrapper(Context)
            );
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal(newDateOfBirth, saved.DateOfBirth);
        }

        [Fact]
        public void CanSetNhsNumberForNewPerson()
        {
            var person = new PersonEntity();

            var newDateOfBirth = 25.April(1876);
            
            new DateOfBirthIdentifierAdapter().Update(
                person,
                new DateOfBirthIdentifier{ DateOfBirth = newDateOfBirth},
                new ContextWrapper(Context)
            );

            Context.Add(person);
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal(newDateOfBirth, saved.DateOfBirth);
        }

        [Fact]
        public void CannotUpdateChangeDateOfBirth()
        {
            var person = saveContext.Add(new PersonEntity {DateOfBirth = 5.April(2018)}).Entity;
            saveContext.SaveChanges();
            

            Assert.Throws<InvalidOperationException>(() => new DateOfBirthIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                new DateOfBirthIdentifier(7.April(2018)), 
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