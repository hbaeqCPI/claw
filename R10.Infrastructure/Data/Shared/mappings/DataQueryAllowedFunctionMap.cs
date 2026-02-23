using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DataQueryAllowedFunctionMap : IEntityTypeConfiguration<DataQueryAllowedFunction>
    {
        public void Configure(EntityTypeBuilder<DataQueryAllowedFunction> builder)
        {
            builder.ToTable("tblDQAllowedFunction");
            builder.HasKey("FnId");
            builder.HasIndex(q => q.FunctionName).IsUnique();
        }
    }
}
