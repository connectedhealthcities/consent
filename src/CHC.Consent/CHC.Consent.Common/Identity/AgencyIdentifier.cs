namespace CHC.Consent.Common.Identity
{
    public class AgencyIdentifier : IdentityBase
    {
        /// <inheritdoc />
        public AgencyIdentifier(long id) : base(id)
        {
        }
        
        public static explicit operator AgencyIdentifier(long id) => new AgencyIdentifier(id);
    }
}