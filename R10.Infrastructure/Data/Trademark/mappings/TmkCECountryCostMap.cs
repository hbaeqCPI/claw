using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCECountryCostMap : IEntityTypeConfiguration<TmkCECountryCost>
    {
        public void Configure(EntityTypeBuilder<TmkCECountryCost> builder)
        {
            builder.ToTable("tblTmkCECountryCost");
            builder.HasKey("CostId");
            builder.HasIndex(c => new { c.Description }).IsUnique();
            builder.HasOne(c => c.TmkCECountrySetup).WithMany(c => c.TmkCECountryCosts).HasPrincipalKey(c => c.CECountryId).HasForeignKey(d => d.CECountryId);
            builder.HasOne(c => c.TmkCEStage).WithMany(c =>c.TmkCECountryCosts).HasPrincipalKey(c => c.Stage).HasForeignKey(d => d.Stage);
        }
    }
}
