using System;
using System.Linq;
using CHC.Consent.NHibernate.Consent;
using NHibernate;
using NHibernate.Util;
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
            this.store = new NHibernateConsentStore(db);
            LoggerProvider.SetLoggersFactory(new OutputLoggerFactory(output));
        }

        [Fact]
        public void CanSaveConsent()
        {
            var subjectIdentifier = "subject identifier" + Guid.NewGuid();
            var evidence = new Evidence {EvidenceKindId = Guid.NewGuid(), TheEvidence = "Some evidence" + Guid.NewGuid()};
            var studyId = Guid.NewGuid();
            var consent = store.RecordConsent(
                studyId,
                subjectIdentifier,
                new [] { evidence });

            using (var session = db.StartSession())
            {
                var saved = session.Get<Consent>(((Consent) consent).Id);
                
                Assert.Equal(studyId, saved.StudyId);
                Assert.Equal(subjectIdentifier, saved.SubjectIdentifier);
                
                Assert.NotEmpty(saved.ProvidedEvidence);
                Assert.Equal(evidence.EvidenceKindId, saved.ProvidedEvidence.First().EvidenceKindId);
                Assert.Equal(evidence.TheEvidence, saved.ProvidedEvidence.First().TheEvidence);
                
                Assert.NotEqual(default(DateTimeOffset), saved.DateProvisionRecorded);
                
                Assert.Null(saved.DateWithdrawlRecorded);
                Assert.Empty(saved.WithdrawnEvidence);
            }
        }
    }
}