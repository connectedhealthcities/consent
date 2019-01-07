using CHC.Consent.EFCore.Security;

namespace CHC.Consent.EFCore.Consent
{
    public class StudyEntity : Securable, IEntity
    {
        public StudyEntity() : base("Study")
        {
        }

        /// <inheritdoc />
        public long Id { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }
    }
}