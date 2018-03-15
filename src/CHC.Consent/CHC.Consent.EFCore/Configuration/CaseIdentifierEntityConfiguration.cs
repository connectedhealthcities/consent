using CHC.Consent.EFCore.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class CaseIdentifierEntityConfiguration : IEntityTypeConfiguration<CaseIdentifierEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<CaseIdentifierEntity> builder)
        {
            builder.ToTable("CaseIdentifier");

            builder.Property<long>("ConsentId").IsRequired();
            builder.HasOne(_ => _.Consent).WithMany().HasForeignKey("ConsentId").IsRequired();

            builder.Property(_ => _.Value).IsRequired().HasMaxLength(int.MaxValue);
            builder.Property(_ => _.Type).IsRequired();
        }
    }
}