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
    public class RelatedProductMap : IEntityTypeConfiguration<RelatedProduct>
    {
        public void Configure(EntityTypeBuilder<RelatedProduct> builder)
        {
            builder.ToTable("tblPrdRelatedProduct");
            builder.HasOne(rp => rp.Product).WithMany(c => c.RelatedProducts);
            builder.HasIndex(rp => new { rp.ProductId, rp.RelProductId }).IsUnique();
        }
    }
}
