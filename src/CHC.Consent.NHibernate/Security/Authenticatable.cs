using System.Collections.Generic;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public abstract class Authenticatable : SecurityPrincipal, IAuthenticatable
    {
        public virtual ICollection<Login> Logins { get; protected set; } = new List<Login>();

        /// <inheritdoc />
        IEnumerator<ILogin> IAuthenticatable.Logins => Logins.GetEnumerator();
    }
}