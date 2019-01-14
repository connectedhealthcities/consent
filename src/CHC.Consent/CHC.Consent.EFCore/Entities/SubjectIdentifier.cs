using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Security;
using JetBrains.Annotations;

namespace CHC.Consent.EFCore.Entities
{
    public class SubjectIdentifierEntity : IEntity, ISecurable
    {
        /// <inheritdoc />
        public long Id { get; [UsedImplicitly] protected set; }
        public long CurrentValue { get; set; }
        public long StudyId { get; set; }
        
        public AccessControlList ACL { get; protected set; } = new AccessControlList { Description = "Subject"};
    }
}