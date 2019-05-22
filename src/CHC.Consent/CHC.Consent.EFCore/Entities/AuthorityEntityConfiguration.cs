using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Entities
{
    public class AuthorityEntityConfiguration : IEntityTypeConfiguration<AuthorityEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AuthorityEntity> builder)
        {
            builder.ToTable("Authority");
            builder.Property(_ => _.Name).IsRequired();
            builder.Property(_ => _.Priority).IsRequired();
            builder.Property(_ => _.SystemName).IsRequired();
            builder.HasIndex(_ => _.SystemName).IsUnique();
        }
    }
}