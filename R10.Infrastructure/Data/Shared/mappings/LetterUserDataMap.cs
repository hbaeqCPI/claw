using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterUserDataMap : IEntityTypeConfiguration<LetterUserData>
    {
        public void Configure(EntityTypeBuilder<LetterUserData> builder)
        {
            builder.ToTable("tblLetUserData");
            builder.HasIndex(d => new {d.LetId, d.DataName}).IsUnique();
            builder.HasOne(d => d.LetterMain).WithMany(l => l.LetterUserData).HasForeignKey(d=>d.LetId).HasPrincipalKey(l=>l.LetId);
        }
    }
}
