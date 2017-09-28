using System;
using CHC.Consent.Common.Core;

namespace CHC.Consent.NHibernate.Consent
{
    public class Evidence : IEvidence
    {
        public virtual Guid Id { get; protected set; }
        /// <inheritdoc />
        public virtual Guid EvidenceKindId { get; set; }

        public virtual string TheEvidence { get; set; }

        //public virtual Consent Consent { get; set; }
        
        /// <inheritdoc />
        string IEvidence.Evidence => TheEvidence;
    }
}