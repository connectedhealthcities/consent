using System.Collections.Generic;

namespace CHC.Consent.Common.Infrastructure
{
    public interface IUserProvider
    {
        string UserName { get; }
        IEnumerable<string> Roles { get; }
    }
}