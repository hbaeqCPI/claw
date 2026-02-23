using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMCostTrackMap : IEntityTypeConfiguration<GMCostTrack>
    {
        public void Configure(EntityTypeBuilder<GMCostTrack> builder)
        {
            builder.ToTable("tblGMCostTracking");
            builder.HasIndex(c => new { c.MatId, c.CostType, c.InvoiceNumber, c.InvoiceDate }).IsUnique();
            builder.HasIndex(c => new { c.CaseNumber, c.CostType, c.InvoiceNumber, c.InvoiceDate }).IsUnique();
            builder.Property(c => c.ExchangeRate).HasDefaultValue(1.0);
            builder.Property(c => c.AllowanceRate).HasDefaultValue(0.0);
            builder.Property(c => c.NetCost)
                  .HasComputedColumnSql("IsNull([ExchangeRate],1) * [InvoiceAmount]");
            builder.HasOne(c => c.GMMatter).WithMany(a => a.CostTrackings).HasForeignKey(c => c.MatId).HasPrincipalKey(a => a.MatId);
            builder.HasOne(c => c.GMCurrencyType).WithMany(cur => cur.CurrencyGMCostTracks).HasForeignKey(c => c.CurrencyType).HasPrincipalKey(cur => cur.CurrencyTypeCode);
            builder.HasOne(c => c.GMCostType).WithMany(ct => ct.GMCostTrackings).HasForeignKey(c => c.CostType).HasPrincipalKey(c => c.CostType);
        }
    }
}
