using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TmkImageCostMap : IEntityTypeConfiguration<TmkImageCost>
    {
        public void Configure(EntityTypeBuilder<TmkImageCost> builder)
        {
            builder.ToTable("tblTmkImageCostTracking");
            builder.HasIndex(ti => new { ti.ParentId, ti.ImageTitle }).IsUnique();
            builder.HasOne(ti => ti.TmkCostTrack).WithMany(ct => ct.Images).HasForeignKey(ti => ti.ParentId).HasPrincipalKey(ct => ct.CostTrackId);

            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.TmkImagesCost).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.TmkImagesCost).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
