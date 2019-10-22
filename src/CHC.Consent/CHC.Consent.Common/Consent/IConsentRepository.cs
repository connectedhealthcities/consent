using System.Collections.Generic;

namespace CHC.Consent.Common.Consent
{
    public interface IConsentRepository
    {
        ConsentIdentity FindActiveConsent(StudySubject studySubject);
        IEnumerable<Consent> GetConsentsForSubject(StudyIdentity studyIdentity, string subjectIdentifier);
        ConsentIdentity AddConsent(Consent consent);
        void WithdrawConsent(StudySubject studySubject, params Evidence[] allEvidence);
    }
}