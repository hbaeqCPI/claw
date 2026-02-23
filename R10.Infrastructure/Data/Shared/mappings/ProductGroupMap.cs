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
    public class ProductGroupMap : IEntityTypeConfiguration<ProductGroup>
    {
        public void Configure(EntityTypeBuilder<ProductGroup> builder)
        {
            builder.ToTable("tblPrdProductGroup");
            builder.Property(pg => pg.ProductGroupId).ValueGeneratedOnAdd();
            builder.Property(pg => pg.ProductGroupId).UseIdentityColumn();
            builder.Property(pg => pg.ProductGroupId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.Property(pg => pg.ProductGroupName).HasColumnName("ProductGroup");
            builder.HasIndex(pg => pg.ProductGroupName).IsUnique();
        }
    }
}
