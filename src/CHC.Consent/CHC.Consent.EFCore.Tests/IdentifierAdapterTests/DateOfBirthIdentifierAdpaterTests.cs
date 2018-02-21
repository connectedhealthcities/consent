
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

        private DateTime GenerateValue() => Random.Date().Date;
        private DateOfBirthIdentifier MakeIdentifier(DateTime? value) => new DateOfBirthIdentifier(value);

        
        [Fact]
        public void CorrectlyFindsPersonByDateOfBirth()
        {
            for (var i = 0; i < 5; i++)
            {
                saveContext.Add(MakeEntity());
            }
            var findValue = GenerateValue();
            saveContext.Add(MakeEntity(findValue));

            saveContext.SaveChanges();

            var foundPeople = new DateOfBirthIdentifierAdapter().Filter(
                readContext.People,
                MakeIdentifier(findValue),
                new ContextWrapper(readContext));

            var found = Assert.Single(foundPeople);
            Assert.NotNull(found);
            Assert.Equal(findValue, GetValue(found));
        }

        private PersonEntity MakeEntity() => MakeEntity(GenerateValue());
        private PersonEntity MakeEntity(DateTime? identifierValue)
        {
            var entity = new PersonEntity();
            SetValue(entity, identifierValue);
            return entity;
        }

        private void SetValue(PersonEntity entity, DateTime? value) => entity.DateOfBirth = value;

        private DateTime? GetValue(PersonEntity entity) => entity.DateOfBirth;
        

        [Fact]
        public void CanSetPersonDateOfBirth()
        {
            var person = saveContext.Add(MakeEntity(default)).Entity;
            saveContext.SaveChanges();

            var newDateOfBirth = GenerateValue();
            
            new DateOfBirthIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                MakeIdentifier(newDateOfBirth), 
                new ContextWrapper(Context)
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
                new ContextWrapper(Context)
            );

            Context.Add(person);
            Context.SaveChanges();

            var saved = readContext.Find<PersonEntity>(person.Id);
            Assert.Equal(newDateOfBirth, GetValue(saved));
        }

        [Fact]
        public void CannotChangeDateOfBirth()
        {
            var person = saveContext.Add(MakeEntity(GenerateValue())).Entity;
            saveContext.SaveChanges();
            

            Assert.Throws<InvalidOperationException>(() => new DateOfBirthIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                MakeIdentifier(GenerateValue()), 
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