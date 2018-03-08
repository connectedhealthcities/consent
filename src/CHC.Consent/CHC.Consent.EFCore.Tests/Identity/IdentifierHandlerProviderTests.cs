using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;
using FakeItEasy;
using Xunit;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class IdentifierHandlerProviderTests
    {
        class Identifier : IPersonIdentifier {}

        class IdentifierHandler : IPersonIdentifierHandler<Identifier>
        {
            /// <inheritdoc />
            public IQueryable<PersonEntity> Filter(
                IQueryable<PersonEntity> people, Identifier value, IStoreProvider stores) =>
                throw new NotImplementedException();

            /// <inheritdoc />
            public IEnumerable<Identifier> Get(PersonEntity person, IStoreProvider stores) =>
                throw new NotImplementedException();

            /// <inheritdoc />
            public bool Update(PersonEntity person, Identifier value, IStoreProvider stores) =>
                throw new NotImplementedException();
        }

        [Fact]
        public void CannotGetHandlerForNonPersonType() =>
            Assert.Throws<ArgumentException>(
                () => new IdentifierHandlerProvider(A.Dummy<IServiceProvider>()).GetHandler(typeof(DateTime)));

        [Fact]
        public void GetsHandlerFromServiceProvider()
        {
            var services = A.Fake<IServiceProvider>(_ => _.Strict());
            var wrapper =
                new PersonIdentifierHandlerWrapper<Identifier>(new IdentifierHandler());
                
            A.CallTo(() => services.GetService(typeof(PersonIdentifierHandlerWrapper<Identifier>))).Returns(wrapper);

            var handler = new IdentifierHandlerProvider(services).GetHandler(typeof(Identifier));
            
            Assert.Equal(wrapper, handler);
        }

    }
}