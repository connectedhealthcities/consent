using CHC.Consent.Security;

namespace CHC.Consent.NHibernate.Security
{
    public class JwtLogin : Login
    {
        /// <remarks>for persistence</remarks>
        protected JwtLogin() {}
        
        /// <inheritdoc />
        public JwtLogin(string issuer, string subject)
        {
            Issuer = issuer;
            Subject = subject;
        }
        
        public virtual string Issuer { get; protected set; }
        public virtual string Subject { get; protected set; }

        /// <inheritdoc />
        protected override bool HasSameBusinessValueAs(Entity compareTo)
        {
            return compareTo is JwtLogin jwtLogin
                   && string.CompareOrdinal(Issuer, jwtLogin.Issuer) == 0
                   && string.CompareOrdinal(Subject, jwtLogin.Subject) == 0;
        }
    }
}