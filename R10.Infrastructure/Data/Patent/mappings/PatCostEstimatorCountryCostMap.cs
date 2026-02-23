using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostEstimatorCountryCostMap : IEntityTypeConfiguration<PatCostEstimatorCountryCost>
    {
        public void Configure(EntityTypeBuilder<PatCostEstimatorCountryCost> builder)
        {
            builder.ToTable("tblPatCostEstimatorCountryCost");
            builder.HasIndex(c => new { c.KeyId, c.CECostId, c.CECCId }).IsUnique();
            builder.HasOne(c => c.CostEstimator).WithMany(c => c.PatCostEstimatorCountryCosts).HasPrincipalKey(c => c.KeyId).HasForeignKey(d => d.KeyId);
        }
    }
}
