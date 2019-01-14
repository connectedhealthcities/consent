using CHC.Consent.Common.Consent;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class StudySubjectEntityConfiguration : IEntityTypeConfiguration<StudySubjectEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<StudySubjectEntity> builder)
        {
            builder.ToTable("StudySubject");

            builder.Property(_ => _.SubjectIdentifier).IsRequired();

            builder.Property<long>("PersonId").IsRequired();
            builder.HasOne(_ => _.Person).WithMany().HasForeignKey("PersonId").IsRequired();
            builder.Property<long>("StudyId").IsRequired();
            builder.HasOne(_ => _.Study).WithMany().HasForeignKey("StudyId").IsRequired().OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasIndex("StudyId", "PersonId").IsUnique();
            builder.HasIndex("StudyId", nameof(StudySubjectEntity.SubjectIdentifier)).IsUnique();

        }
    }
}