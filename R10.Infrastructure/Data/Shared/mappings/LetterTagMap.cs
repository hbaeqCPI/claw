using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterTagMap : IEntityTypeConfiguration<LetterTag>
    {
        public void Configure(EntityTypeBuilder<LetterTag> builder)
        {
            builder.ToTable("tblLetterTag");
            builder.Property(d => d.LetTagId).ValueGeneratedOnAdd();
            builder.HasOne(d => d.Letter).WithMany(f => f.LetterTags).HasForeignKey(d => d.LetId).HasPrincipalKey(d => d.LetId);
        }
    }
}
