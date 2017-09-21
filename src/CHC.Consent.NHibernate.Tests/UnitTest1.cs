using System;
using System.Xml.Linq;
using CHC.Consent.Common.Identity;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Test1()
        {
            var configuration = new IdentityConfiguration(_ => output.WriteLine(_));

            var session = configuration.Session;


            var simpleKind = new IdentityKind{Format = IdentityKindFormat.Simple, ExternalId = Guid.NewGuid().ToString("N")};
            session.SaveOrUpdate(simpleKind);
            session.SaveOrUpdate(new SimpleIdentity{ IdentityKind = simpleKind, Value = Guid.NewGuid().ToString("N")});

            var compositeKind =
                new IdentityKind {Format = IdentityKindFormat.Composite, ExternalId = Guid.NewGuid().ToString("N")};
            session.SaveOrUpdate(compositeKind);
            session.SaveOrUpdate(new CompositeIdentity{IdentityKind = compositeKind, CompositeValue = XDocument.Parse("<p>test</p>")});
            
            session.Flush();
            
        }
    }
}