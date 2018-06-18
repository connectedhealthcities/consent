using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Security;

namespace CHC.Consent.EFCore.Consent
{
    public class StudyEntity : IEntity, ISecurable
    {
        /// <inheritdoc />
        public long Id { get; set; }
        public string Name { get; set; }
        public AccessControlList ACL { get; set; } = new AccessControlList { Description = "Study"};
    }
}