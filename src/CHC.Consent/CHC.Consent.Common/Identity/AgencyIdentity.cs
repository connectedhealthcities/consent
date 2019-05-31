namespace CHC.Consent.Common.Identity
{
    public class AgencyIdentity : IdentityBase
    {
        /// <inheritdoc />
        public AgencyIdentity(long id) : base(id)
        {
        }
        
        public static explicit operator AgencyIdentity(long id) => new AgencyIdentity(id);
    }
}