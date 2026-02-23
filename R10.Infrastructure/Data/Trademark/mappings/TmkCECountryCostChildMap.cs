using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCECountryCostChildMap : IEntityTypeConfiguration<TmkCECountryCostChild>
    {
        public void Configure(EntityTypeBuilder<TmkCECountryCostChild> builder)
        {
            builder.ToTable("tblTmkCECountryCostChild");
            builder.HasKey("CCId");
            builder.HasIndex(c => new { c.CDescription }).IsUnique();
            builder.HasOne(c => c.TmkCECountryCost).WithMany(c => c.TmkCECountryCostChilds).HasPrincipalKey(c => c.CostId).HasForeignKey(d => d.CostId);
            builder.HasOne(c => c.TmkCurrencyType).WithMany(c => c.CurrencyTmkCECountryCostChilds).HasPrincipalKey(c => c.CurrencyTypeCode).HasForeignKey(d => d.CurrencyType);
        }
    }
}
