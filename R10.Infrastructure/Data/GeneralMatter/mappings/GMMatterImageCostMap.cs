using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterImageCostMap : IEntityTypeConfiguration<GMMatterImageCost>
    {
        public void Configure(EntityTypeBuilder<GMMatterImageCost> builder)
        {
            builder.ToTable("tblGMMatterImageCostTracking");
            builder.HasIndex(ii => new { ii.ParentId, ii.ImageTitle }).IsUnique();
            builder.HasOne(ii => ii.GMCostTrack).WithMany(ct => ct.Images).HasForeignKey(ii => ii.ParentId).HasPrincipalKey(ct => ct.CostTrackId);

            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.GMMatterImagesCost).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.GMMatterImagesCost).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
