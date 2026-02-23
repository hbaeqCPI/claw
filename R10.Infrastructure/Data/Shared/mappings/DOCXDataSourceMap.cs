
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXDataSourceMap : IEntityTypeConfiguration<DOCXDataSource>
    {
        public void Configure(EntityTypeBuilder<DOCXDataSource> builder)
        {
            builder.ToTable("tblDOCXDataSource");
        }
    }
}
