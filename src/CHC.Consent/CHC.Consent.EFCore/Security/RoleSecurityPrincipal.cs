namespace CHC.Consent.EFCore.Security
{
    public class RoleSecurityPrincipal : SecurityPrinicipal
    {
        public ConsentRole Role { get; set; }
    }
}