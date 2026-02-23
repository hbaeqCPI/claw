using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostEstimatorCostMap : IEntityTypeConfiguration<TmkCostEstimatorCost>
    {
        public void Configure(EntityTypeBuilder<TmkCostEstimatorCost> builder)
        {
            builder.ToTable("tblTmkCostEstimatorCost");
            builder.HasKey("CECostId");
            builder.HasIndex(c => new { c.Description }).IsUnique();
            builder.HasMany(g => g.TmkCostEstimatorCountryCosts).WithOne(q => q.TmkCostEstimatorCost).HasForeignKey(q => q.CECostId).HasPrincipalKey(g => g.CECostId);
            builder.HasOne(c => c.TmkCECountrySetup).WithMany(c => c.TmkCostEstimatorCosts).HasPrincipalKey(c => c.CECountryId).HasForeignKey(d => d.CECountryId);
        }
    }
}
