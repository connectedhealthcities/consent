﻿using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CHC.Consent.EFCore.Configuration
{
    public class IdentifierEntityConfiguration : IEntityTypeConfiguration<IdentifierEntity>
    {
        /// <inheritdoc />
        public void Configure(EntityTypeBuilder<IdentifierEntity> builder)
        {
            builder.HasOne(_ => _.Person).WithMany().IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Property(_ => _.Value).HasMaxLength(int.MaxValue);
            builder.Property(_ => _.ValueType).IsRequired();
            builder.Property(_ => _.TypeName).IsRequired();
        }
    }

    
}