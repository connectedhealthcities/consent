
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
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

        private DateTime CreateDateOfBirth() => Random.Date().Date;
        private DateOfBirthIdentifier MakeIdentifier(DateTime? value) => new DateOfBirthIdentifier(value);


        [Fact]
        public void CorrectlyFindsPersonByDateOfBirth()
        {
            saveContext.AddRange(5.Of(MakeEntity));
            
            var findValue = CreateDateOfBirth();
            saveContext.Add(MakeEntity(findValue));

            saveContext.SaveChanges();

            var foundPeople = new DateOfBirthIdentifierAdapter().Filter(
                readContext.People,
                MakeIdentifier(findValue),
                new ContextStoreProvider(readContext));

            var found = Assert.Single(foundPeople);
            Assert.NotNull(found);
            Assert.Equal(findValue, found.DateOfBirth);
        }

        private PersonEntity MakeEntity() => MakeEntity(CreateDateOfBirth());

        private PersonEntity MakeEntity(DateTime? identifierValue)
        {
            var entity = new PersonEntity();
            entity.DateOfBirth = identifierValue;
            return entity;
        }


        [Fact]
        public void CanSetPersonDateOfBirth()
        {
            var person = saveContext.Add(MakeEntity(default)).Entity;
            saveContext.SaveChanges();

            var newDateOfBirth = CreateDateOfBirth();
            
            new DateOfBirthIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                MakeIdentifier(newDateOfBirth), 
                new ContextStoreProvider(Context)
            );
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal(newDateOfBirth, saved.DateOfBirth);
        }

        [Fact]
        public void CanSetDateOfBirthForNewPerson()
        {
            var person = MakeEntity(default);

            var newDateOfBirth = 25.April(1876);
            
            new DateOfBirthIdentifierAdapter().Update(
                person,
                MakeIdentifier(newDateOfBirth),
                new ContextStoreProvider(Context)
            );

            Context.Add(person);
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal(newDateOfBirth, saved.DateOfBirth);
        }

        [Fact]
        public void CannotChangeDateOfBirth()
        {
            var person = saveContext.Add(MakeEntity(CreateDateOfBirth())).Entity;
            saveContext.SaveChanges();
            

            Assert.Throws<InvalidOperationException>(() => new DateOfBirthIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                MakeIdentifier(CreateDateOfBirth()), 
                new ContextStoreProvider(Context)
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