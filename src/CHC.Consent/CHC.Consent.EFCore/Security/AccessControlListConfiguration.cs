using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Security
{
    public class AccessControlListConfiguration : IEntityTypeConfiguration<AccessControlList>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<AccessControlList> builder)
        {
            builder.Property(_ => _.Description).IsRequired().HasMaxLength(512);
        }
    }
}