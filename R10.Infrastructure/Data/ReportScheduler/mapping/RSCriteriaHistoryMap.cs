using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSCriteriaHistoryMap : IEntityTypeConfiguration<RSCriteriaHistory>
    {
        public void Configure(EntityTypeBuilder<RSCriteriaHistory> builder)
        {
            builder.ToTable("tblRSCriteriaHistory");
            builder.HasKey("CritHistoryId");
            builder.HasIndex(a => new { a.LogId, a.FieldName }).IsUnique();
        }
    }
}
