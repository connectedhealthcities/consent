using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Core;
using CHC.Consent.Core;
using CHC.Consent.NHibernate.Security;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Consent
{
    public class Consent : Entity, IConsent
    {
        protected Consent()
        {
        }

        /// <inheritdoc />
        public Consent(Subject subject, DateTimeOffset dateProvisionRecorded, IEnumerable<Evidence> providedEvidence)
        {
            Subject = subject;
            DateProvisionRecorded = dateProvisionRecorded;
            ProvidedEvidence = providedEvidence.ToList();
        }

        public virtual Subject Subject { get; protected set; }

        /// <inheritdoc />
        ISubject IConsent.Subject => Subject;
        
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

        public static Evidence MakeEvidence(IEvidence evidence)
        {
            return new Evidence
            {
                EvidenceKindId = evidence.EvidenceKindId,
                TheEvidence = evidence.Evidence,
            };
        }

        public virtual Authenticatable Authenticatable { get; set; }
        public virtual DateTimeOffset Date { get; set; }
    }
}