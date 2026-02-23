using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostTrackMap : IEntityTypeConfiguration<PatCostTrack>
    {
        public void Configure(EntityTypeBuilder<PatCostTrack> builder)
        {
            builder.ToTable("tblPatCostTracking");
            builder.HasIndex(c => new { c.AppId, c.CostType, c.InvoiceNumber, c.InvoiceDate }).IsUnique();
            builder.HasIndex(c => new { c.CaseNumber, c.Country, c.SubCase, c.CostType, c.InvoiceNumber, c.InvoiceDate }).IsUnique();
            builder.Property(c => c.ExchangeRate).HasDefaultValue(1.0);
            builder.Property(c => c.AllowanceRate).HasDefaultValue(0.0);
            builder.Property(c => c.NetCost)
                  .HasComputedColumnSql("IsNull([ExchangeRate],1) * [InvoiceAmount]");
            builder.HasOne(c => c.CountryApplication).WithMany(a => a.CostTrackings).HasForeignKey(c => c.AppId).HasPrincipalKey(a=>a.AppId);
            builder.HasOne(c => c.PatCurrencyType).WithMany(cur => cur.CurrencyPatCostTracks).HasForeignKey(c => c.CurrencyType).HasPrincipalKey(cur => cur.CurrencyTypeCode);
            builder.HasOne(c => c.PatCountry).WithMany(c => c.PatCostTrackings).HasForeignKey(ct => ct.Country);
            builder.HasOne(c => c.PatCostType).WithMany(c => c.PatCostTrackings).HasForeignKey(ct => ct.CostType).HasPrincipalKey(c => c.CostType);
        }
    }
}
