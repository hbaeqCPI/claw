using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ReportParameterMap : IEntityTypeConfiguration<ReportParameter>
    {
        public void Configure(EntityTypeBuilder<ReportParameter> builder)
        {
            builder.ToTable("tmpReportParameters");
            builder.HasKey(x => new { x.Id });
            builder.Property(prop => prop.Id).HasDefaultValueSql("newid()");
        }
    }
}
