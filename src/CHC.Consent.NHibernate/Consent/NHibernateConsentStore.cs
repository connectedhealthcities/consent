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
            var session = sessionAccessor();
            var study = session.Get<Study>(studyId);
            var subject = session.Query<Subject>().SingleOrDefault(_ => _.Study == study && _.Identifier == subjectIdentifier)
            ?? new Subject(study, subjectIdentifier);
            session.SaveOrUpdate(subject);

            var consent = new Consent(
                subject,
                DateTimeOffset.UtcNow,
                evidence.Select(Consent.MakeEvidence));
            
            session.Save(consent);

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
