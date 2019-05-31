using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class PersonAgencyIdConfiguration : IEntityTypeConfiguration<PersonAgencyId>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PersonAgencyId> builder)
        {
            builder.HasOne<PersonEntity>().WithMany().HasForeignKey(_ => _.PersonId).IsRequired();
            builder.HasOne<AgencyEntity>().WithMany().HasForeignKey(_ => _.AgencyId).IsRequired();
            builder.Property(_ => _.SpecificId).IsRequired();
            builder.HasIndex(_ => new {_.AgencyId, _.SpecificId}).IsUnique();
        }
    }
}