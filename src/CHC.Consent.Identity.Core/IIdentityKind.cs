using System;

namespace CHC.Consent.Identity.Core
{
    public interface IIdentityKind
    {
        Guid Id { get; }
        string ExternalId { get; }
    }
}