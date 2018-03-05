using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests
{
    public class IndentityRepositoryTests : DbTests
    {
        [Fact]
        public void ThisTestIsInTheWrongPlace()
        {
            var personOne = new PersonEntity();
            var personOneNhsNumber = new PersonIdentifierEntity
            {
                Person = personOne,
                TypeName = NhsNumberIdentifier.TypeName,
                Value = "5555555",
                ValueType = "string"
            };
            var personTwo = new PersonEntity();
            var personTwoNhsNumber = new PersonIdentifierEntity
            {
                Person = personTwo,
                TypeName = NhsNumberIdentifier.TypeName,
                Value = "666666",
                ValueType = "string"
            }; 

            Context.AddRange(personOne, personTwo);
            Context.AddRange(personOneNhsNumber, personTwoNhsNumber);
            Context.SaveChanges();


            var identityHandler = new PersonIdentifierHandler<NhsNumberIdentifier>(new NhsNumberIdentifierMarshaller(), NhsNumberIdentifier.TypeName);
            var handlerProvider = A.Fake<IIdentifierHandlerProvider>();
            A.CallTo(() => handlerProvider.GetHandler(typeof(NhsNumberIdentifier))).Returns(new PersonIdentifierHandlerWrapper<NhsNumberIdentifier>(identityHandler));
            
            var storeProvider = (IStoreProvider)new ContextStoreProvider (CreateNewContextInSameTransaction());

            var repository = new IdentityRepository(
                storeProvider.Get<PersonEntity>(), 
                A.Dummy<ITypeRegistry<IPersonIdentifier>>(),
                handlerProvider,
                storeProvider);

            Assert.Equal(repository.FindPersonBy(new NhsNumberIdentifier(personOneNhsNumber.Value)), personOne);
            Assert.Equal(repository.FindPersonBy(new NhsNumberIdentifier(personTwoNhsNumber.Value)), personTwo);
            Assert.Null(repository.FindPersonBy(new NhsNumberIdentifier("7787773")));
        }

        /// <inheritdoc />
        public IndentityRepositoryTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
        }
    }
}