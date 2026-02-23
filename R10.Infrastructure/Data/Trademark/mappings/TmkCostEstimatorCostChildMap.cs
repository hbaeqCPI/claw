using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostEstimatorCostChildMap : IEntityTypeConfiguration<TmkCostEstimatorCostChild>
    {
        public void Configure(EntityTypeBuilder<TmkCostEstimatorCostChild> builder)
        {
            builder.ToTable("tblTmkCostEstimatorCostChild");
            builder.HasKey("CECCId");
            builder.HasIndex(c => new { c.CDescription }).IsUnique();
            builder.HasOne(c => c.TmkCostEstimatorCost).WithMany(c => c.TmkCostEstimatorCostChilds).HasPrincipalKey(c => c.CECostId).HasForeignKey(d => d.CECostId);
            builder.HasMany(g => g.TmkCostEstimatorCountryCosts).WithOne(q => q.TmkCostEstimatorCostChild).HasForeignKey(q => q.CECCId).HasPrincipalKey(g => g.CECCId);
        }
    }
}
