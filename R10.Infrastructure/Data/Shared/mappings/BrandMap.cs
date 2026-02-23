using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class BrandMap : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("tblPrdBrand");
            builder.Property(b => b.BrandId).ValueGeneratedOnAdd();
            builder.Property(b => b.BrandId).UseIdentityColumn();
            builder.Property(b => b.BrandId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.Property(b => b.BrandName).HasColumnName("Brand");
            builder.HasIndex(b => b.BrandName).IsUnique();

        }
    }
}
