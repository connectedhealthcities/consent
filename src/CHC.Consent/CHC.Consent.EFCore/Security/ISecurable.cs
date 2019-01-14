namespace CHC.Consent.EFCore.Security
{
    public interface ISecurable
    {
        AccessControlList ACL { get; }
    }
}