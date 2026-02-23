using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostEstimatorCostSubMap : IEntityTypeConfiguration<PatCostEstimatorCostSub>
    {
        public void Configure(EntityTypeBuilder<PatCostEstimatorCostSub> builder)
        {
            builder.ToTable("tblPatCostEstimatorCostSub");
            builder.HasKey("CESubId");
            builder.HasIndex(c => new { c.SDescription }).IsUnique();
            builder.HasOne(c => c.PatCostEstimatorCostChild).WithMany(c => c.PatCostEstimatorCostSubs).HasPrincipalKey(c => c.CECCId).HasForeignKey(d => d.CECCId);
            builder.HasMany(g => g.PatCostEstimatorCountryCosts).WithOne(q => q.PatCostEstimatorCostSub).HasForeignKey(q => q.CESubId).HasPrincipalKey(g => g.CESubId);
        }
    }
}