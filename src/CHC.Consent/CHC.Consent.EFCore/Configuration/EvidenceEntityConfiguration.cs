using CHC.Consent.EFCore.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace CHC.Consent.EFCore.Configuration
{
    public class EvidenceEntityConfiguration : 
        IEntityTypeConfiguration<EvidenceEntity>,
        IEntityTypeConfiguration<GivenEvidenceEntity>, 
        IEntityTypeConfiguration<WithdrawnEvidenceEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<EvidenceEntity> builder)
        {
            builder.ToTable("Evidence");
            
            builder.Property(_ => _.Value).HasMaxLength(int.MaxValue).IsRequired();
            builder.Property(_ => _.Type).IsRequired();
            builder.Ignore(_ => _.Consent);
            builder.HasDiscriminator()
                .HasValue<GivenEvidenceEntity>("Given")
                .HasValue<WithdrawnEvidenceEntity>("Withdrawn");
        }

        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<GivenEvidenceEntity> builder)
        {
            builder.HasOne(_ => _.Consent).WithMany(_ => _.GivenEvidence).HasForeignKey("ConsentId").IsRequired();
            
            builder.HasBaseType<EvidenceEntity>();
            
        }

        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<WithdrawnEvidenceEntity> builder)
        {
            builder.HasOne(_ => _.Consent).WithMany(_ => _.WithdrawnEvidence).HasForeignKey("ConsentId").IsRequired();
            builder.HasBaseType<EvidenceEntity>();
        }
    }
}