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
    public class ProductCategoryMap : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("tblPrdProductCategory");
            builder.Property(pc => pc.ProductCategoryId).ValueGeneratedOnAdd();
            builder.Property(pc => pc.ProductCategoryId).UseIdentityColumn();
            builder.Property(pc => pc.ProductCategoryId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.Property(pc => pc.ProductCategoryName).HasColumnName("ProductCategory");
            builder.HasIndex(pc => pc.ProductCategoryName).IsUnique();
        }
    }
}
