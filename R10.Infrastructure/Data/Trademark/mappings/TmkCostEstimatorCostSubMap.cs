using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostEstimatorCostSubMap : IEntityTypeConfiguration<TmkCostEstimatorCostSub>
    {
        public void Configure(EntityTypeBuilder<TmkCostEstimatorCostSub> builder)
        {
            builder.ToTable("tblTmkCostEstimatorCostSub");
            builder.HasKey("CESubId");
            builder.HasIndex(c => new { c.SDescription }).IsUnique();
            builder.HasOne(c => c.TmkCostEstimatorCostChild).WithMany(c => c.TmkCostEstimatorCostSubs).HasPrincipalKey(c => c.CECCId).HasForeignKey(d => d.CECCId);
            builder.HasMany(g => g.TmkCostEstimatorCountryCosts).WithOne(q => q.TmkCostEstimatorCostSub).HasForeignKey(q => q.CESubId).HasPrincipalKey(g => g.CESubId);
        }
    }
}
