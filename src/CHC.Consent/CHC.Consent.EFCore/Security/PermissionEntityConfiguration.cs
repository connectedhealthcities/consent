using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Security
{
    public class PermissionEntityConfiguration : IEntityTypeConfiguration<PermissionEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<PermissionEntity> map)
        {
            map.ToTable("Permission");
            map.Property(_ => _.Access).IsRequired().HasMaxLength(512);
        }
    }
}