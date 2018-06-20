namespace CHC.Consent.EFCore.Security
{
    public class PermissionEntity : IEntity
    {
        public long Id { get; set; }
        public string Access { get; set; }
    }
}