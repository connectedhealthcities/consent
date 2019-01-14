using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Security
{
    public class AccessControlEntityConfiguration : IEntityTypeConfiguration<AccessControlEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AccessControlEntity> map)
        {
            map.ToTable("AccessControl");
            map.Property<long>("SecurityPrincipalId").IsRequired();
            map.HasOne(_ => _.Prinicipal).WithMany(_ => _.Access).HasForeignKey("SecurityPrincipalId").IsRequired();
            map.Property<long>("AccessControlListId").IsRequired();
            map.HasOne(_ => _.ACL).WithMany(_ => _.Entries).HasForeignKey("AccessControlListId").IsRequired();
            map.Property<long>("PermissionId").IsRequired();
            map.HasOne(_ => _.Permission).WithMany().HasForeignKey("PermissionId").IsRequired();
        }
    }
}