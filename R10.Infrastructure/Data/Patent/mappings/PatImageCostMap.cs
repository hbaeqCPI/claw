using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatImageCostMap : IEntityTypeConfiguration<PatImageCost>
    {
        public void Configure(EntityTypeBuilder<PatImageCost> builder)
        {
            builder.ToTable("tblPatImageCostTracking");
            builder.HasIndex(ii => new { ii.ParentId, ii.ImageTitle }).IsUnique();
            builder.HasOne(ii => ii.PatCostTrack).WithMany(ct => ct.Images).HasForeignKey(ii => ii.ParentId).HasPrincipalKey(ct => ct.CostTrackId);

            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.PatImagesCost).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.PatImagesCost).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
