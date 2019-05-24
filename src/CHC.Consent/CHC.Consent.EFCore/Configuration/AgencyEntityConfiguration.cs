using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class AgencyEntityConfiguration : IEntityTypeConfiguration<AgencyEntity>,
        IEntityTypeConfiguration<AgencyFieldEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AgencyEntity> builder)
        {
            builder.ToTable("Agency");
            builder.Property(_ => _.Name).IsRequired();
            builder.Property(_ => _.SystemName).IsRequired();
            builder.HasIndex(_ => _.SystemName).IsUnique();
        }

        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AgencyFieldEntity> builder)
        {
            builder.ToTable("AgencyField");
            const string agencyId = "AgencyId";
            builder.Property<long>(agencyId).IsRequired();
            builder.HasOne(_ => _.Agency).WithMany(_ => _.Fields).HasForeignKey(agencyId).IsRequired();
            builder.HasOne(_ => _.Identifier).WithMany().IsRequired().HasForeignKey("IdentifierDefinitionId");
            builder.Property(_ => _.Order).IsRequired();
            builder.HasIndex(agencyId, nameof(AgencyFieldEntity.Order)).IsUnique();
        }
    }
}