
using System;
using System.Collections.Generic;
using System.Linq;
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
        private XmlIdentifierMarshaller<DateOfBirthIdentifier> marshaller;

        /// <inheritdoc />
        public DateOfBirthIdentifierAdapterTests (ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            saveContext = CreateNewContextInSameTransaction();
            readContext = CreateNewContextInSameTransaction();
            marshaller = new XmlIdentifierMarshaller<DateOfBirthIdentifier>("date");
        }

        private DateTime CreateDateOfBirth() => Random.Date().Date;
        private DateOfBirthIdentifier MakeIdentifier(DateTime? value) => new DateOfBirthIdentifier(value);


        [Fact]
        public void CorrectlyFindsPersonByDateOfBirth()
        {
            5.Times(AddPersonWithDateOfBirth);
            
            var findValue = CreateDateOfBirth();
            var findMe = AddPersonWithDateOfBirth(findValue);

            saveContext.SaveChanges();

            var foundPeople = new DateOfBirthIdentifierAdapter().Filter(
                readContext.People,
                MakeIdentifier(findValue),
                new ContextStoreProvider(readContext));

            var found = Assert.Single(foundPeople);
            Assert.NotNull(found);
            Assert.Equal(findMe.Id, found.Id);
        }

        private PersonEntity AddPersonWithDateOfBirth() => AddPersonWithDateOfBirth(Random.Date());

        private PersonEntity AddPersonWithDateOfBirth(DateTime date)
        {
            var personEntity = saveContext.Add(new PersonEntity()).Entity;
            saveContext.Add(new PersonIdentifierEntity
            {
                Person = personEntity,
                TypeName = DateOfBirthIdentifier.TypeName,
                Value = marshaller.MarshalledValue(MakeIdentifier(date.Date)),
                ValueType = "date"
            });

            return personEntity;
        }


        [Fact]
        public void CanSetPersonDateOfBirth()
        {
            var person = saveContext.Add(new PersonEntity()).Entity;
            saveContext.SaveChanges();

            var newDateOfBirth = CreateDateOfBirth();
            
            new DateOfBirthIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                MakeIdentifier(newDateOfBirth), 
                new ContextStoreProvider(Context)
            );
            Context.SaveChanges();

            AssertHasActiveDateOfBirth(person, newDateOfBirth);
        }

        [Fact]
        public void CanSetDateOfBirthForNewPerson()
        {
            var person = new PersonEntity();

            var newDateOfBirth = 25.April(1876);
            
            new DateOfBirthIdentifierAdapter().Update(
                person,
                MakeIdentifier(newDateOfBirth),
                new ContextStoreProvider(Context)
            );

            Context.Add(person);
            Context.SaveChanges();

            AssertHasActiveDateOfBirth(person, newDateOfBirth);
        }

        [Fact]
        public void StoresOldDateOfBirth()
        {
            var originalDateOfBirth = CreateDateOfBirth();
            var person = AddPersonWithDateOfBirth(originalDateOfBirth);
            saveContext.SaveChanges();

            var newDateOfBirth = CreateDateOfBirth();
            while (newDateOfBirth == originalDateOfBirth) newDateOfBirth = CreateDateOfBirth();
            
            new DateOfBirthIdentifierAdapter().Update(
                Context.Find<PersonEntity>(person.Id),
                MakeIdentifier(newDateOfBirth),
                new ContextStoreProvider(Context)
            );
            Context.SaveChanges();

            var storedDateOfBirth = GetDatesOfBirthFor(person).ToArray();
            
            Assert.Collection(
                storedDateOfBirth,
                _ => { AssertIsDateOfBirth(originalDateOfBirth, _); Assert.NotNull(_.Deleted);},
                _ => { AssertIsDateOfBirth(newDateOfBirth, _); Assert.Null(_.Deleted); }
                );
        }

        private void AssertIsDateOfBirth(DateTime expectedDateOfBirth, PersonIdentifierEntity single)
        {
            Assert.NotNull(single);
            Assert.Equal(expectedDateOfBirth, marshaller.Unmarshall(marshaller.ValueType, single.Value).DateOfBirth);
        }

        private void AssertHasActiveDateOfBirth(PersonEntity person, DateTime newDateOfBirth)
        {
            var found = GetDatesOfBirthFor(person).Where(_ => _.Deleted == null).ToArray();
            AssertIsDateOfBirth(newDateOfBirth, Assert.Single(found));
        }

        private IEnumerable<PersonIdentifierEntity> GetDatesOfBirthFor(PersonEntity person)
        {
            return readContext.Set<PersonIdentifierEntity>().Where(_ => _.Person.Id == person.Id && _.TypeName == DateOfBirthIdentifier.TypeName).OrderBy(_ => _.Created);
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