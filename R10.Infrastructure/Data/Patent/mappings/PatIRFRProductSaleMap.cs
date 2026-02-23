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
    public class PatIRFRProductSaleMap : IEntityTypeConfiguration<PatIRFRProductSale>
    {
        public void Configure(EntityTypeBuilder<PatIRFRProductSale> builder)
        {
            builder.ToTable("tblPatIRFRProductSale");
            builder.Property(c => c.ProductSaleId).ValueGeneratedOnAdd();
            builder.Property(m => m.ProductSaleId).UseIdentityColumn();
            builder.HasOne(h => h.PatIRFRTurnOver).WithMany(c => c.PatIRFRProductSales).IsRequired(false).HasForeignKey(pi => pi.Year).HasPrincipalKey(i => i.Year);
            builder.HasOne(i => i.FRRemuneration).WithMany(ps => ps.PatIRFRProductSales).HasForeignKey(ps => ps.FRRemunerationId).HasPrincipalKey(i => i.FRRemunerationId);
        }
    }
}
