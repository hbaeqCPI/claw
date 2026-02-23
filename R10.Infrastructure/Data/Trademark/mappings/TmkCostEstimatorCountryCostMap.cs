using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostEstimatorCountryCostMap : IEntityTypeConfiguration<TmkCostEstimatorCountryCost>
    {
        public void Configure(EntityTypeBuilder<TmkCostEstimatorCountryCost> builder)
        {
            builder.ToTable("tblTmkCostEstimatorCountryCost");
            builder.HasIndex(c => new { c.KeyId, c.CECostId, c.CECCId }).IsUnique();
            builder.HasOne(c => c.CostEstimator).WithMany(c => c.TmkCostEstimatorCountryCosts).HasPrincipalKey(c => c.KeyId).HasForeignKey(d => d.KeyId);
        }
    }
}
