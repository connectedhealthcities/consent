using System;
using CHC.Consent.Common.Core;

namespace CHC.Consent.NHibernate.Consent
{
    public class EvidenceKind : IEvidenceKind
    {
        public virtual Guid Id { get; private set; }

        /// <inheritdoc />
        public string ExternalId { get; set; }
    }
}