using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCECountryCostSubMap : IEntityTypeConfiguration<TmkCECountryCostSub>
    {
        public void Configure(EntityTypeBuilder<TmkCECountryCostSub> builder)
        {
            builder.ToTable("tblTmkCECountryCostSub");
            builder.HasKey("SubId");
            builder.HasIndex(c => new { c.SDescription }).IsUnique();            
            builder.HasOne(c => c.TmkCECountryCostChild).WithMany(c =>c.TmkCECountryCostSubs).HasPrincipalKey(c => c.CCId).HasForeignKey(d => d.CCId);
        }
    }
}
