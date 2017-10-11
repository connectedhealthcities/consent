using System.Collections.Generic;

namespace CHC.Consent.Security
{
    public interface IAuthenticatable : ISecurityPrincipal
    {
        IEnumerator<ILogin> Logins { get; }
    }
}