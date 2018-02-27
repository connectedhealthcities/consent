using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    internal class BradfordHospitalNumberEntityConfiguration : IEntityTypeConfiguration<BradfordHospitalNumberEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<BradfordHospitalNumberEntity> builder)
        {
            builder.ToTable("BradfordHosptialNumber");

            builder
                .HasOne(_ => _.Person)
                .WithMany().IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}