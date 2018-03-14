using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.EFCore.Consent
{
    public class StudyEntity : IEntity
    {
        /// <inheritdoc />
        public long Id { get; set; }
        public string Name { get; set; }
    }
}