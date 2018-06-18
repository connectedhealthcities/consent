using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.EFCore.Security
{
    public class PermissionEntity : IEntity
    {
        public long Id { get; set; }
        public string Access { get; set; }
    }
}