using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Core;
using NHibernate;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.Consent
{
    public class NHibernateConsentStore : IConsentStore, IEvidenceKindStore
    {
        private readonly Func<ISession> sessionAccessor;

        /// <inheritdoc />
        public NHibernateConsentStore(Func<ISession> sessionAccessor)
        {
            this.sessionAccessor = sessionAccessor;
        }

        /// <inheritdoc />
        public IConsent RecordConsent(Guid studyId, string subjectIdentifier, IEnumerable<IEvidence> evidence)
        {
            var consent = new Consent
            {
                DateProvisionRecorded = DateTimeOffset.UtcNow,
                StudyId = studyId,
                SubjectIdentifier = subjectIdentifier
            };

            consent.AddProvidedEvidence(evidence);

            sessionAccessor().Save(consent);

            return consent;
        }
        
        /// <inheritdoc />
        public IEvidenceKind FindEvidenceKindByExternalId(string externalId)
        {
            return sessionAccessor().Query<EvidenceKind>().FirstOrDefault(_ => _.ExternalId == externalId);
        }

        public IEvidenceKind AddEvidenceKind(string externalId)
        {
            var evidenceKind = new EvidenceKind { ExternalId = externalId };
            sessionAccessor().Save(evidenceKind);
            return evidenceKind;
        }

    }

}
