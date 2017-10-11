using System;
using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public abstract class Login : Entity, ILogin
    {
        protected Login() {}

        public virtual User User { get; protected set; }

        public virtual void SetUser(User user)
        {
            User = user;
        }
    }
}