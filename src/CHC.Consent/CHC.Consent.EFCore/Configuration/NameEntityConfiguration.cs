using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class NameEntityConfiguration : IEntityTypeConfiguration<NameEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<NameEntity> builder)
        {
            builder.ToTable("PersonName");
            builder.HasOne(_ => _.Person).WithMany().IsRequired();
        }
    }
}