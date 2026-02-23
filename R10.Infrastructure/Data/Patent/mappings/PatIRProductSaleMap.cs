using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIRProductSaleMap : IEntityTypeConfiguration<PatIRProductSale>
    {
        public void Configure(EntityTypeBuilder<PatIRProductSale> builder)
        {
            builder.ToTable("tblPatIRProductSale");
            builder.Property(c => c.ProductSaleId).ValueGeneratedOnAdd();
            builder.Property(m => m.ProductSaleId).UseIdentityColumn();
            builder.HasOne(h => h.PatIRTurnOver).WithMany(c => c.PatIRProductSales).IsRequired(false).HasForeignKey(pi => pi.Year).HasPrincipalKey(i => i.Year);
            builder.HasOne(i => i.Remuneration).WithMany(ps => ps.PatIRProductSales).HasForeignKey(ps => ps.RemunerationId).HasPrincipalKey(i => i.RemunerationId);
        }
    }
}
