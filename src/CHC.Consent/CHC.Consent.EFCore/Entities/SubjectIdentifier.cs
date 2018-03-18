using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Consent;
using JetBrains.Annotations;

namespace CHC.Consent.EFCore.Entities
{
    public class SubjectIdentifierEntity : IEntity
    {
        /// <inheritdoc />
        public long Id { get; [UsedImplicitly] protected set; }
        public long CurrentValue { get; set; }
        public long StudyId { get; set; }
    }
}