using CHC.Consent.Common.Consent;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class SubjectIdentifierEntityConfiguration : IEntityTypeConfiguration<SubjectIdentifierEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<SubjectIdentifierEntity> map)
        {
            map.ToTable("SubjectIdentifiers");
            map
                .HasOne(typeof(StudyEntity)).WithMany()
                .IsRequired()
                .HasForeignKey(nameof(SubjectIdentifierEntity.StudyId))
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}