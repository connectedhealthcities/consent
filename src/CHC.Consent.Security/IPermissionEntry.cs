namespace CHC.Consent.Security
{
    public interface IPermissionEntry
    {
        ISecurityPrincipal Principal { get; }
        IPermisson Permisson { get; }
        ISecurable Securable { get; }
    }
}