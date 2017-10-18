using System;
using System.Linq.Expressions;
using CHC.Consent.Identity.Core;
using CHC.Consent.NHibernate.Identity;
using Moq;
using Xunit;

namespace CHC.Consent.NHibernate.Tests.Identity
{
    public class IdentityQueryGeneratorTests
    {
        class UnknownMatch : IMatch {}

        [Fact]
        public void UsesIdentityGeneratorForMatchIdentity()
        {
            Expression<Func<NHibernate.Identity.Identity, bool>> query = _ => true;
            
            var provider = new Mock<IIdentityKindHelperProvider>();
            var helper = new Mock<IIdentityKindHelper>();

            var identityMatch = new Mock<IIdentityMatch>();
            var identity = new Mock<IIdentity>().Object;
            identityMatch.SetupGet(_ => _.Match).Returns(identity);

            helper.Setup(_ => _.CreateMatchQuery(identity)).Returns(query);
            provider.Setup(_ => _.GetHelperFor(identity)).Returns(helper.Object);
            
            var generator = new IdentityQueryGenerator(provider.Object);

            var generatedQuery = generator.CreateMatchQuery(identityMatch.Object);

            Assert.Equal(generatedQuery, query);
        }

        [Fact]
        public void ThrowsExceptionForUnknownMatch()
        {
            var generator = new IdentityQueryGenerator(Mock.Of<IIdentityKindHelperProvider>());

            Assert.Throws<NotImplementedException>(() => generator.CreateMatchQuery(new UnknownMatch()));
        }
    }
}