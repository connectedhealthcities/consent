using System;
using System.Linq;
using CHC.Consent.NHibernate.Consent;
using CHC.Consent.Testing.NHibernate;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.NHibernate.Tests.Consent
{
    using Consent = NHibernate.Consent.Consent;
    [Collection(DatabaseCollection.Name)]
    public class ConsentStoreTest
    {
        private readonly DatabaseFixture db;
        private readonly NHibernateConsentStore store;

        public ConsentStoreTest(DatabaseFixture db, ITestOutputHelper output)
        {
            this.db = db;
            store = new NHibernateConsentStore(db.SessionAccessor);
        }

        [Fact]
        public void CanSaveConsent()
        {
            var subjectIdentifier = "subject identifier" + Guid.NewGuid();
            var evidence = new Evidence {EvidenceKindId = Guid.NewGuid(), TheEvidence = "Some evidence" + Guid.NewGuid()};
            var studyId = Guid.NewGuid();


            var consent = db.InTransactionalUnitOfWork(
                () => store.RecordConsent(
                    studyId,
                    subjectIdentifier,
                    new[] {evidence})
                    );

            using (var session = db.StartSession())
            {
                var saved = session.Get<Consent>(((Consent) consent).Id);
                
                Assert.Equal(studyId, saved.Subject.Study.Id);
                Assert.Equal(subjectIdentifier, saved.Subject.Identifier);
                
                Assert.NotEmpty(saved.ProvidedEvidence);
                Assert.Equal(evidence.EvidenceKindId, saved.ProvidedEvidence.First().EvidenceKindId);
                Assert.Equal(evidence.TheEvidence, saved.ProvidedEvidence.First().TheEvidence);
                
                Assert.NotEqual(default(DateTimeOffset), saved.DateProvisionRecorded);
                
                Assert.Null(saved.DateWithdrawlRecorded);
                Assert.Empty(saved.WithdrawnEvidence);
            }
        }

        [Fact]
        public void CanFindEvidenceKinds()
        {
            var externalId = "urn:chc:consent:evidence-kind:persistence:test:" + Guid.NewGuid();
            var id = db.InTransactionalUnitOfWork(s => s.Save(new EvidenceKind {ExternalId = externalId}));

            var found = db.InTransactionalUnitOfWork(() => store.FindEvidenceKindByExternalId(externalId));
            Assert.NotNull(found);
            Assert.Equal(id, found.Id);
            Assert.Equal(externalId, found.ExternalId);
        }
    }
}