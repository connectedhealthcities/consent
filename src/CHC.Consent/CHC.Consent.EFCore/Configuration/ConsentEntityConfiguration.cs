using CHC.Consent.EFCore.Consent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class ConsentEntityConfiguration : IEntityTypeConfiguration<ConsentEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<ConsentEntity> builder)
        {
            builder.ToTable("Consent");
            builder.Property<long>("StudySubjectId").IsRequired();
            builder.HasOne(_ => _.StudySubject).WithMany().HasForeignKey("StudySubjectId").IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.Property<long>("GivenByPersonId").IsRequired();
            builder.HasOne(_ => _.GivenBy).WithMany().HasForeignKey("GivenByPersonId").IsRequired();

            builder
                .HasIndex("StudySubjectId", nameof(ConsentEntity.DateProvided), nameof(ConsentEntity.DateWithdrawn))
                .IsUnique();
        }
    }
}