using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostEstimatorCostChildMap : IEntityTypeConfiguration<PatCostEstimatorCostChild>
    {
        public void Configure(EntityTypeBuilder<PatCostEstimatorCostChild> builder)
        {
            builder.ToTable("tblPatCostEstimatorCostChild");
            builder.HasKey("CECCId");
            builder.HasIndex(c => new { c.CDescription }).IsUnique();
            builder.HasOne(c => c.PatCostEstimatorCost).WithMany(c => c.PatCostEstimatorCostChilds).HasPrincipalKey(c => c.CECostId).HasForeignKey(d => d.CECostId);
            builder.HasMany(g => g.PatCostEstimatorCountryCosts).WithOne(q => q.PatCostEstimatorCostChild).HasForeignKey(q => q.CECCId).HasPrincipalKey(g => g.CECCId);
        }
    }
}
