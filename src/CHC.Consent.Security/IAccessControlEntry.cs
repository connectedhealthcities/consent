namespace CHC.Consent.Security
{
    public interface IAccessControlEntry
    {
        ISecurityPrincipal Principal { get; }
        IPermisson Permission { get; }
        IAccessControlList AccessControlList { get; }
    }
}