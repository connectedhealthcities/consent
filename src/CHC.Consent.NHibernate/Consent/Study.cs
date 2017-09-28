using System;
using CHC.Consent.Common.Core;

namespace CHC.Consent.NHibernate.Consent
{
    public class Study : IStudy
    {
        /// <inheritdoc />
        public virtual Guid Id { get; protected set; }
    }
}