using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterImageActMap : IEntityTypeConfiguration<GMMatterImageAct>
    {
        public void Configure(EntityTypeBuilder<GMMatterImageAct> builder)
        {
            builder.ToTable("tblGMMatterImageAct");
            builder.HasIndex(ai => new { ai.ParentId, ai.ImageTitle }).IsUnique();
            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.GMMatterImagesAct).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.GMMatterImagesAct).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
