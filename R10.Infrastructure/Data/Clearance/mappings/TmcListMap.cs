using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcListMap : IEntityTypeConfiguration<TmcList>
    {
        public void Configure(EntityTypeBuilder<TmcList> builder)
        {
            builder.ToTable("tblTmcList");
            builder.HasIndex(k => new { k.TmcId, k.ListItem }).IsUnique();
            builder.HasOne(k => k.Clearance).WithMany(t => t.ListItems).HasForeignKey(k => k.TmcId).HasPrincipalKey(t => t.TmcId);
        }
    }
}