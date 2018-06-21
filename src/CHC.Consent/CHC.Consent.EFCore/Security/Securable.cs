namespace CHC.Consent.EFCore.Security
{
    public class Securable : ISecurable
    {
        /// <inheritdoc />
        public AccessControlList ACL { get; set; } = new AccessControlList { Description = "Person"};
    }
}