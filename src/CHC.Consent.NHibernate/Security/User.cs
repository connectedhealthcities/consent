using System.Collections.ObjectModel;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class User : Authenticatable, IUser
    {
        public virtual void AddLogin(Login login)
        {
            Logins.Add(login);
        }
    }
}