using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSPrintOptionControlMap : IEntityTypeConfiguration<RSPrintOptionControl>
    {
        public void Configure(EntityTypeBuilder<RSPrintOptionControl> builder)
        {
            builder.ToTable("tblRSPrintOptionControl");
            builder.HasKey("ParamId");
            builder.HasIndex(a => new { a.ReportId, a.OptionName }).IsUnique();
        }
    }
}
