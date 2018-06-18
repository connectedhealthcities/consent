namespace CHC.Consent.EFCore.Security
{
    public class UserSecurityPrincipal : SecurityPrinicipal
    {
        public ConsentUser User { get; set; }
    }
}