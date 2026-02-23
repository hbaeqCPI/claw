using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TmkImageMap : IEntityTypeConfiguration<TmkImage>
    {
        public void Configure(EntityTypeBuilder<TmkImage> builder)
        {
            builder.ToTable("vwTmkImage");
            //builder.HasIndex(ti => new { ti.ParentId, ti.ImageTitle }).IsUnique();
            builder.HasOne(ti => ti.TmkTrademark).WithMany(t => t.Images).HasForeignKey(ti => ti.ParentId).HasPrincipalKey(t => t.TmkId);
            builder.HasOne(ti => ti.DocType).WithMany(t => t.TmkImages).HasForeignKey(ti => ti.DocTypeId).HasPrincipalKey(t => t.DocTypeId);


        }
    }
}
