using System;
using CHC.Consent.Common.Core;

namespace CHC.Consent.NHibernate.Consent
{
    public class EvidenceKind : IEvidenceKind
    {
        public virtual Guid Id { get; protected set; }

        /// <inheritdoc />
        public virtual string ExternalId { get; set; }
    }
}