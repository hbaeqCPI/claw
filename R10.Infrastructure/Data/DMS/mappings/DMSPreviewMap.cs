using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSPreviewMap : IEntityTypeConfiguration<DMSPreview>
    {
        public void Configure(EntityTypeBuilder<DMSPreview> builder)
        {
            builder.ToTable("tblDMSPreview");
            builder.Property(r => r.DMSPreviewId).ValueGeneratedOnAdd();
            builder.HasIndex(r => new { r.DMSId, r.DMSPreviewId }).IsUnique();
            builder.HasOne(r => r.Disclosure).WithMany(r => r.Previews).HasForeignKey(d => d.DMSId).HasPrincipalKey(r => r.DMSId);            
            builder.HasOne(r => r.Contact).WithMany(r => r.Previews).HasForeignKey(r => r.PreviewerId).HasPrincipalKey(c => c.ContactID);
            builder.HasOne(r => r.Inventor).WithMany(r => r.Previews).HasForeignKey(r => r.PreviewerId).HasPrincipalKey(c => c.InventorID);
        }
    }
}
