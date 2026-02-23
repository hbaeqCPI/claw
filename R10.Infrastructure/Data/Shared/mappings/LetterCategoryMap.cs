using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterCategoryMap : IEntityTypeConfiguration<LetterCategory>
    {
        public void Configure(EntityTypeBuilder<LetterCategory> builder)
        {
            builder.ToTable("tblLetCategory");
            builder.Property(c => c.LetCatId).ValueGeneratedOnAdd();
        }
    }
}
