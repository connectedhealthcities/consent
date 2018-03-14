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
            
            builder.HasOne(_ => _.Person).WithMany().IsRequired();
            builder.HasOne(_ => _.Study).WithMany().IsRequired();
        }
    }
}