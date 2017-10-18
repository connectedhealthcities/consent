using System;
using CHC.Consent.Common.Core;

namespace CHC.Consent.NHibernate.Consent
{
    public class Evidence : Entity, IEvidence
    {
        /// <inheritdoc />
        public virtual Guid EvidenceKindId { get; set; }

        public virtual string TheEvidence { get; set; }

        /// <inheritdoc />
        string IEvidence.Evidence => TheEvidence;
    }
}