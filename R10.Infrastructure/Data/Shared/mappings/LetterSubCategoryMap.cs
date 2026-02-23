using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class LetterSubCategoryMap : IEntityTypeConfiguration<LetterSubCategory>
    {
        public void Configure(EntityTypeBuilder<LetterSubCategory> builder)
        {
            builder.ToTable("tblLetSubCategory");
            builder.Property(c => c.LetSubCatId).ValueGeneratedOnAdd();
        }
    }
}
