using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterMainMap : IEntityTypeConfiguration<LetterMain>
    {
        public void Configure(EntityTypeBuilder<LetterMain> builder)
        {
            builder.ToTable("tblLetMain");
            builder.HasKey("LetId");
            builder.HasIndex(l => l.LetName).IsUnique();
            builder.HasOne(l => l.SystemScreen).WithMany(s => s.LetterMain).HasForeignKey(l => l.ScreenId).HasPrincipalKey(s=>s.ScreenId);
            builder.HasOne(l => l.LetterCategory).WithMany(s => s.LetterMains).HasForeignKey(l => l.LetCatId).HasPrincipalKey(s => s.LetCatId);
            builder.HasOne(l => l.LetterSubCategory).WithMany(s => s.LetterMains).HasForeignKey(l => l.LetSubCatId).HasPrincipalKey(s => s.LetSubCatId);
            builder.HasMany(l => l.LetterRecordSources).WithOne(r => r.LetterMain).HasForeignKey(l=>l.LetId).HasPrincipalKey(r=>r.LetId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
