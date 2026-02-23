using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.DTOs;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ProductLatestTopSaleDTOMap : IEntityTypeConfiguration<ProductLatestTopSaleDTO>
    {
        public void Configure(EntityTypeBuilder<ProductLatestTopSaleDTO> builder)
        {
            builder.ToView("vwPrdLatestTopSale");
            builder.HasOne(s => s.Product).WithOne(c => c.ProductLatestTopSale).HasForeignKey<ProductLatestTopSaleDTO>(s => s.ProductId).HasPrincipalKey<Product>(p => p.ProductId);
        }
    }
}
