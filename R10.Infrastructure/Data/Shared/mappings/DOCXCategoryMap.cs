using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXCategoryMap : IEntityTypeConfiguration<DOCXCategory>
    {
        public void Configure(EntityTypeBuilder<DOCXCategory> builder)
        {
            builder.ToTable("tblDOCXCategory");
            builder.Property(c => c.DOCXCatId).ValueGeneratedOnAdd();
        }
    }
}
