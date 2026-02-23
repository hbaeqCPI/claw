using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSPrintOptionMap : IEntityTypeConfiguration<RSPrintOption>
    {
        public void Configure(EntityTypeBuilder<RSPrintOption> builder)
        {
            builder.ToTable("tblRSPrintOption");
            builder.HasKey("SchedParamId");
            builder.HasIndex(a => new { a.TaskId, a.OptionName }).IsUnique();
        }
    }
}
