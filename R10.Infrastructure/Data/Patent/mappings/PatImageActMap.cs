using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatImageActMap : IEntityTypeConfiguration<PatImageAct>
    {
        public void Configure(EntityTypeBuilder<PatImageAct> builder)
        {
            builder.ToTable("tblPatImageAct");
            builder.HasIndex(ai => new { ai.ParentId, ai.ImageTitle }).IsUnique();
            builder.HasOne(ai => ai.PatActionDue).WithMany(i => i.Images).HasForeignKey(ai => ai.ParentId).HasPrincipalKey(a => a.ActId);

            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.PatImagesAct).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.PatImagesAct).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
