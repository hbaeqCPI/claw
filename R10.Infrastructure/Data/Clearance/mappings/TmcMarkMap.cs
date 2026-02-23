using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcMarkMap : IEntityTypeConfiguration<TmcMark>
    {
        public void Configure(EntityTypeBuilder<TmcMark> builder)
        {
            builder.ToTable("tblTmcMark");
            builder.HasIndex(k => new { k.TmcId, k.MarkName, k.MarkType }).IsUnique();
            builder.HasOne(k => k.Clearance).WithMany(t => t.Marks).HasForeignKey(k => k.TmcId).HasPrincipalKey(t => t.TmcId);
        }
    }
}