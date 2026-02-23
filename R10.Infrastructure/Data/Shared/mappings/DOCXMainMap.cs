
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXMainMap : IEntityTypeConfiguration<DOCXMain>
    {
        public void Configure(EntityTypeBuilder<DOCXMain> builder)
        {
            builder.ToTable("tblDOCXMain");
            builder.HasKey("DOCXId");
            builder.HasIndex(l => l.DOCXName).IsUnique();
            builder.HasOne(l => l.SystemScreen).WithMany(s => s.DOCXMain).HasForeignKey(l => l.ScreenId).HasPrincipalKey(s=>s.ScreenId);
            builder.HasOne(l => l.DOCXCategory).WithMany(s => s.DOCXMains).HasForeignKey(l => l.DOCXCatId).HasPrincipalKey(s => s.DOCXCatId);
            builder.HasMany(l => l.DOCXRecordSources).WithOne(r => r.DOCXMain).HasForeignKey(l=>l.DOCXId).HasPrincipalKey(r=>r.DOCXId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
