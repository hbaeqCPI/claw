using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterImageMap : IEntityTypeConfiguration<GMMatterImage>
    {
        public void Configure(EntityTypeBuilder<GMMatterImage> builder)
        {
            builder.ToTable("tblGMMatterImage");
            builder.HasIndex(gi => new { gi.ParentId, gi.ImageTitle }).IsUnique();
            builder.HasOne(gi => gi.GMMatter).WithMany(g => g.Images).HasForeignKey(gi => gi.ParentId).HasPrincipalKey(g => g.MatId);

            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.GMMatterImages).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.GMMatterImages).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
