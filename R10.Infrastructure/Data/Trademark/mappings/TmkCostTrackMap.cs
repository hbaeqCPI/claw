using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{

    public class TmkCostTrackMap : IEntityTypeConfiguration<TmkCostTrack>
    {
        public void Configure(EntityTypeBuilder<TmkCostTrack> builder)
        {
            builder.ToTable("tblTmkCostTracking");
            builder.HasIndex(c => new { c.TmkId, c.CostType, c.InvoiceNumber, c.InvoiceDate}).IsUnique();
            builder.HasIndex(c => new { c.CaseNumber, c.Country, c.SubCase, c.CostType, c.InvoiceNumber, c.InvoiceDate}).IsUnique();
            builder.Property(c => c.ExchangeRate).HasDefaultValue(1.0);
            builder.Property(c => c.AllowanceRate).HasDefaultValue(0.0);
            builder.Property(c => c.NetCost)
                  .HasComputedColumnSql("IsNull([ExchangeRate],1) * [InvoiceAmount]");
            builder.HasOne(c => c.TmkTrademark).WithMany(t => t.CostTrackings).HasForeignKey(c => c.TmkId).HasPrincipalKey(t => t.TmkId);
            builder.HasOne(c => c.TmkCurrencyType).WithMany(cur => cur.CurrencyTmkCostTracks).HasForeignKey(c => c.CurrencyType).HasPrincipalKey(cur => cur.CurrencyTypeCode);
            builder.HasOne(c => c.TmkCountry).WithMany(c => c.TmkCostTrackings).HasForeignKey(ct => ct.Country).HasPrincipalKey(c => c.Country);
            builder.HasOne(c => c.TmkCostType).WithMany(c => c.TmkCostTrackings).HasForeignKey(ct => ct.CostType).HasPrincipalKey(c => c.CostType);
        }
    }
}
