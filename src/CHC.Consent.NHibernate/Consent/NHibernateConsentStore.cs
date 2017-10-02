using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Core;
using NHibernate.Linq;

namespace CHC.Consent.NHibernate.Consent
{
    public class NHibernateConsentStore : IConsentStore, IEvidenceKindStore
    {
        private readonly ISessionFactory sessionFactory;

        /// <inheritdoc />
        public NHibernateConsentStore(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
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

            sessionFactory.AsTransaction(_ => _.Save(consent));

            return consent;
        }


        /// <inheritdoc />
        public IEvidenceKind FindEvidenceKindByExternalId(string externalId)
        {
            return sessionFactory.AsTransaction(
                s => s.Query<EvidenceKind>().FirstOrDefault(_ => _.ExternalId == externalId));
        }

        public IEvidenceKind AddEvidenceKind(string externalId) =>
            sessionFactory.AsTransaction(
                s =>
                {
                    var evidenceKind = new EvidenceKind {ExternalId = externalId};
                    s.Save(evidenceKind);
                    return evidenceKind;
                }
            );

    }
}