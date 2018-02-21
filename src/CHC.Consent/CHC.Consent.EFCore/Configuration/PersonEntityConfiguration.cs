using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class PersonEntityConfiguration : IEntityTypeConfiguration<PersonEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PersonEntity> modelBuilder)
        {
            modelBuilder.Ignore(_ => _.BirthOrder);
            modelBuilder
                .Property(_ => _.BirthOrderValue)
                .HasColumnName(nameof(PersonEntity.BirthOrder));
        }
    }
}