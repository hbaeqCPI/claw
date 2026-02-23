using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class QECategoryMap : IEntityTypeConfiguration<QECategory>
    {
        public void Configure(EntityTypeBuilder<QECategory> builder)
        {
            builder.ToTable("tblQECategory");
            builder.Property(c => c.QECatId).ValueGeneratedOnAdd();
        }
    }
}
