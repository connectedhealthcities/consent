namespace CHC.Consent.Common.Identity
{
    public class AuthorityIdentity : IdentityBase
    {
        /// <inheritdoc />
        public AuthorityIdentity(long id) : base(id)
        {
        }
        
        public static explicit operator AuthorityIdentity(long id) => new AuthorityIdentity(id);
    }
}