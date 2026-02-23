using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatImageAppMap : IEntityTypeConfiguration<PatImageApp>
    {
        public void Configure(EntityTypeBuilder<PatImageApp> builder)
        {
            builder.ToTable("tblPatImageApp");
            builder.HasIndex(ai => new { ai.ParentId, ai.ImageTitle }).IsUnique();
            builder.HasOne(ai => ai.CountryApplication).WithMany(i => i.Images).HasForeignKey(ai => ai.ParentId).HasPrincipalKey(a => a.AppId);

            builder.HasOne(ti => ti.FileHandler).WithMany(f => f.PatImagesApp).HasForeignKey(ti => ti.FileID).HasPrincipalKey(t => t.FileID);
            builder.HasOne(ti => ti.ImageType).WithMany(t => t.PatImagesApp).HasForeignKey(ti => ti.ImageTypeId).HasPrincipalKey(t => t.ImageTypeId);
        }
    }
}
