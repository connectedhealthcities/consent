using CHC.Consent.Common.Consent;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class StudyConfiguration : IEntityTypeConfiguration<StudyEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<StudyEntity> builder)
        {
            builder.ToTable("Study");
            builder.Property(_ => _.Name).IsRequired();
        }
    }
}