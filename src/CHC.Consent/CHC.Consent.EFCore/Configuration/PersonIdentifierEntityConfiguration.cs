using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class PersonIdentifierEntityConfiguration : IEntityTypeConfiguration<PersonIdentifierEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PersonIdentifierEntity> builder)
        {
            builder.ToTable("PersonIdentifier");
            builder.Property(_ => _.Value).HasMaxLength(int.MaxValue);
            builder.Property(_ => _.ValueType).IsRequired();
            builder.Property(_ => _.TypeName).IsRequired();
        }
    }

    
}