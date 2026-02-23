using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DataQueryCategoryMap : IEntityTypeConfiguration<DataQueryCategory>
    {
        public void Configure(EntityTypeBuilder<DataQueryCategory> builder)
        {
            builder.ToTable("tblDQCategory");
            builder.Property(c => c.DQCatId).ValueGeneratedOnAdd();
        }
    }
}
