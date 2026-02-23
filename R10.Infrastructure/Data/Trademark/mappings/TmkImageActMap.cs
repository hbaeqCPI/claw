using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TmkImageActMap : IEntityTypeConfiguration<TmkImageAct>
    {
        public void Configure(EntityTypeBuilder<TmkImageAct> builder)
        {
            builder.ToTable("tblTmkImageAct");
            builder.HasIndex(ii => new { ii.ParentId, ii.ImageTitle }).IsUnique();
            builder.HasOne(ti => ti.TmkActionDue).WithMany(t => t.Images).HasForeignKey(ti => ti.ParentId).HasPrincipalKey(t => t.ActId);

            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.TmkImagesAct).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.TmkImagesAct).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
