using System;
using CHC.Consent.Common.Core;

namespace CHC.Consent.NHibernate.Consent
{
    public class EvidenceKind : Entity, IEvidenceKind
    {
        /// <inheritdoc />
        public virtual string ExternalId { get; set; }
    }
}