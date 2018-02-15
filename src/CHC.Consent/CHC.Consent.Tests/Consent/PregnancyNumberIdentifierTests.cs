using System;
using System.Linq;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Identifiers;
using Xunit;

namespace CHC.Consent.Tests.Consent
{
    using Consent = Common.Consent.Consent;
    public class PregnancyNumberIdentifierTests
    {
        [Fact]
        public void SetsPregancyNumberOnConsent()
        {
            var pregnancyId = Guid.NewGuid().ToString();
            var identifier = new PregnancyNumberIdentifier(pregnancyId);
            Consent consent = Create.Consent;

            identifier.Update(consent);
            
            
            Assert.Equal(pregnancyId, consent.PregnancyNumber);
        }

        [Fact]
        public void CanFilterConsentsByPregnancyId()
        {
            StudySubject studySubject = Create.StudySubject;
            var createConsent = Create.Consent.WithStudySubject(studySubject);
            
            var consents = new Consent[]
            {
                createConsent.WithPregnancyNumber("12333"),
                createConsent.WithPregnancyNumber("445552"),
                createConsent.WithPregnancyNumber("213855"),
                createConsent.WithPregnancyNumber("FIND ME"),
                createConsent.WithPregnancyNumber("88765"),
            }.AsQueryable();

            var pregnancyNumberIdentifier = new PregnancyNumberIdentifier("FIND ME");

            var matched = consents.Where(pregnancyNumberIdentifier.CreateMatchIdentifier()).ToArray();

            var foundConsent = Assert.Single(matched);
            Assert.NotNull(foundConsent);
            Assert.Equal("FIND ME", foundConsent.PregnancyNumber);
        }
    }
}