using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSHistoryMap : IEntityTypeConfiguration<RSHistory>
    {
        public void Configure(EntityTypeBuilder<RSHistory> builder)
        {
            builder.ToTable("tblRSHistory");
            builder.HasKey("LogId");
            builder.Property(s => s.LogId).ValueGeneratedOnAdd();
        }
    }
}
