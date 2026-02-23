using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSPrintOptionHistoryMap : IEntityTypeConfiguration<RSPrintOptionHistory>
    {
        public void Configure(EntityTypeBuilder<RSPrintOptionHistory> builder)
        {
            builder.ToTable("tblRSPrintOptionHistory");
            builder.HasKey("OptionHistoryId");
            builder.HasIndex(a => new { a.LogId, a.OptionName }).IsUnique();
        }
    }
}
