using System;
using System.Collections.Generic;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.DependencyInjection;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
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


            var identityHandler = new PersonIdentifierPersistanceHandler<NhsNumberIdentifier>(
                new NhsNumberIdentifierMarshaller(),
                NhsNumberIdentifier.TypeName,
                new XunitLogger<PersonIdentifierPersistanceHandler<NhsNumberIdentifier>>(outputHelper, "test"));
            
            var handlerProvider = A.Fake<IIdentifierHandlerProvider>();
            A.CallTo(() => handlerProvider.GetPersistanceHandler(typeof(NhsNumberIdentifier))).Returns(new PersonIdentifierPersistanceHandlerWrapper<NhsNumberIdentifier>(identityHandler));
            
            var storeProvider = (IStoreProvider)new ContextStoreProvider (CreateNewContextInSameTransaction());

            var repository = new IdentityRepository(
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

    public class IndentityRepositoryFastTests
    {
        [Fact]
        public void GroupsIdentityUpdatesByIdentityType()
        {
            var registryBuilder = new PersonIdentifierRegistryOptions();

            registryBuilder.Add<NhsNumberIdentifier>(_ => _.WithMarshaller<NhsNumberIdentifierMarshaller>());
            registryBuilder.Add<MedwayContactNumberIdentifier>(_ => _.WithXmlMarshaller("Bib4All.MedwayNumber"));

            var storeProvider = A.Dummy<IStoreProvider>();

            var handlerProvider = A.Fake<IIdentifierHandlerProvider>();

            var handlerFake = A.Fake<IPersonIdentifierPersistanceHandler>();
            
            
            A.CallTo(() => handlerProvider.GetPersistanceHandler(A<Type>._)).Returns(handlerFake);
            
            var repository = new IdentityRepository(
                registryBuilder.CreateTypeRegistry(),
                handlerProvider,
                storeProvider);

            var nhsNumberIdentifier = new NhsNumberIdentifier("1234");
            var mobileNumberIdentifier = new MedwayContactNumberIdentifier {Type = "Mobile", Number = "1"};
            var homeNumberIdentifier = new MedwayContactNumberIdentifier {Type = "Home", Number = "2"};
            repository.CreatePerson(
                new IPersonIdentifier[]
                {
                    nhsNumberIdentifier,
                    mobileNumberIdentifier,
                    homeNumberIdentifier,
                }
            );

            A.CallTo(
                    () => handlerFake.Update(
                        A<PersonEntity>._,
                        A<IEnumerable<IPersonIdentifier>>.That.IsSameSequenceAs(
                            mobileNumberIdentifier,
                            homeNumberIdentifier),
                        storeProvider))
                .MustHaveHappened();
            
            A.CallTo(
                    () => handlerFake.Update(
                        A<PersonEntity>._,
                        A<IEnumerable<IPersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier),
                        storeProvider))
                .MustHaveHappened();
                
        }
    }
}