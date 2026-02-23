using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatImageInvMap : IEntityTypeConfiguration<PatImageInv>
    {
        public void Configure(EntityTypeBuilder<PatImageInv> builder)
        {
            builder.ToTable("tblPatImageInv");
            builder.HasIndex(ii => new { ii.ParentId, ii.ImageTitle }).IsUnique();
            builder.HasOne(ii => ii.Invention).WithMany(i => i.Images).HasForeignKey(ii => ii.ParentId).HasPrincipalKey(i => i.InvId);

            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.PatImagesInv).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.PatImagesInv).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
