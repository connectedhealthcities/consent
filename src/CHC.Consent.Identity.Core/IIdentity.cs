using System;

namespace CHC.Consent.Identity.Core
{
    public interface IIdentity
    {
        Guid IdentityKindId { get; }
    }
}