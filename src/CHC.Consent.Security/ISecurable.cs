namespace CHC.Consent.Security
{
    public interface ISecurable
    {
        IAccessControlList AccessControlList { get; }
    }
}