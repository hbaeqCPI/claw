using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostTrackInvMap : IEntityTypeConfiguration<PatCostTrackInv>
    {
        public void Configure(EntityTypeBuilder<PatCostTrackInv> builder)
        {
            builder.ToTable("tblPatCostTrackingInv");
            builder.HasIndex(c => new { c.InvId, c.CostType, c.InvoiceNumber, c.InvoiceDate }).IsUnique();
            builder.HasIndex(c => new { c.CaseNumber, c.CostType, c.InvoiceNumber, c.InvoiceDate }).IsUnique();
            builder.Property(c => c.ExchangeRate).HasDefaultValue(1.0);
            builder.Property(c => c.NetCost)
                  .HasComputedColumnSql("IsNull([ExchangeRate],1) * [InvoiceAmount]");
            builder.HasOne(c => c.Invention).WithMany(a => a.CostTrackings).HasForeignKey(c => c.InvId).HasPrincipalKey(a => a.InvId);
            builder.HasOne(c => c.PatCurrencyType).WithMany(cur => cur.CurrencyPatCostTrackInvs).HasForeignKey(c => c.CurrencyType).HasPrincipalKey(cur => cur.CurrencyTypeCode);
            builder.HasOne(c => c.PatCostType).WithMany(c => c.PatCostTrackingInvs).HasForeignKey(ct => ct.CostType).HasPrincipalKey(c => c.CostType);
        }
    }
}