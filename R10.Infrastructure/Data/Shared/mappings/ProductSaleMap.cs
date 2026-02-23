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
    public class ProductSaleMap : IEntityTypeConfiguration<ProductSale>
    {
        public void Configure(EntityTypeBuilder<ProductSale> builder)
        {
            builder.ToTable("tblPrdSale");
            builder.HasOne(t => t.Product).WithMany(t => t.ProductSales);
        }
    }
}
