using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Consent
{
    public class Consent : Entity, IConsent, INHibernateSecurable
    {
        public Consent()
        {
            Acl = new AccessControlList(this);
        }

        /// <inheritdoc />
        public virtual Guid StudyId { get; set; }

        /// <inheritdoc />
        public virtual string SubjectIdentifier { get; set; }

        /// <inheritdoc />
        public virtual DateTimeOffset DateProvisionRecorded { get; set; }

        public virtual ICollection<Evidence> ProvidedEvidence { get; protected set; } = new List<Evidence>();

        /// <inheritdoc />
        IEnumerable<IEvidence> IConsent.ProvidedEvidence => this.ProvidedEvidence;

        /// <inheritdoc />
        public virtual DateTimeOffset? DateWithdrawlRecorded { get; set; }

        public virtual ICollection<Evidence> WithdrawnEvidence { get; set; } = new List<Evidence>();

        /// <inheritdoc />
        IEnumerable<IEvidence> IConsent.WithdrawnEvidence => WithdrawnEvidence;

        public virtual void AddProvidedEvidence(IEnumerable<IEvidence> evidence)
        {
            ProvidedEvidence = ProvidedEvidence.Concat(evidence.Select(MakeEvidence)).ToList();
        }
        
        private Evidence MakeEvidence(IEvidence evidence)
        {
            return new Evidence
            {
                EvidenceKindId = evidence.EvidenceKindId,
                TheEvidence = evidence.Evidence,
            };
        }

        /// <inheritdoc />
        IAccessControlList ISecurable.AccessControlList => Acl;

        /// <inheritdoc />
        public virtual AccessControlList Acl { get; protected set; }
    }
}