namespace CHC.Consent.EFCore.Security
{
    public class AccessControlEntity : IEntity
    {
        public long Id { get; set; }
        
        public AccessControlList ACL { get; set; }
        
        public SecurityPrinicipal Prinicipal { get; set; }
        
        /// <summary>
        /// What access do they have?
        /// </summary>
        public PermissionEntity Permission { get; set; }
    }
}