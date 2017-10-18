namespace CHC.Consent.Security
{
    public interface IAccessControlEntry
    {
        ISecurityPrincipal Principal { get; }
        IPermisson Permisson { get; }
        IAccessControlList AccessControlList { get; }
    }
}