using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class MedwayNameEntityConfiguration : IEntityTypeConfiguration<MedwayNameEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<MedwayNameEntity> builder)
        {
            builder.HasOne(_ => _.Person).WithMany().IsRequired();
        }
    }
}