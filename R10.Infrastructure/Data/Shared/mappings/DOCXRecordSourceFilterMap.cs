using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DOCXRecordSourceFilterMap : IEntityTypeConfiguration<DOCXRecordSourceFilter>
    {
        public void Configure(EntityTypeBuilder<DOCXRecordSourceFilter> builder)
        {
            builder.ToTable("tblDOCXRecSourceFilter");
            builder.HasOne(f => f.DOCXRecordSource).WithMany(r => r.DOCXRecordSourceFilters);
        }

    }
}
