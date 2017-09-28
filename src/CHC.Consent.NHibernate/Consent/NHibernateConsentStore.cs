using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;

namespace CHC.Consent.NHibernate.Consent
{
    public class NHibernateConsentStore : IConsentStore
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

        
    }
}