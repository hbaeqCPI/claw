using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostEstimatorCostMap : IEntityTypeConfiguration<PatCostEstimatorCost>
    {
        public void Configure(EntityTypeBuilder<PatCostEstimatorCost> builder)
        {
            builder.ToTable("tblPatCostEstimatorCost");
            builder.HasKey("CECostId");
            builder.HasIndex(c => new { c.Description }).IsUnique();
            builder.HasMany(g => g.PatCostEstimatorCountryCosts).WithOne(q => q.PatCostEstimatorCost).HasForeignKey(q => q.CECostId).HasPrincipalKey(g => g.CECostId);
            builder.HasOne(c => c.PatCECountrySetup).WithMany(c =>c.PatCostEstimatorCosts).HasPrincipalKey(c => c.CECountryId).HasForeignKey(d => d.CECountryId);
        }
    }
}
