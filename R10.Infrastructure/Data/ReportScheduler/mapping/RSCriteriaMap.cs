using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSCriteriaMap : IEntityTypeConfiguration<RSCriteria>
    {
        public void Configure(EntityTypeBuilder<RSCriteria> builder)
        {
            builder.ToTable("tblRSCriteria");
            builder.HasKey("SchedCritId");
            builder.HasIndex(a => new { a.TaskId, a.FieldName }).IsUnique();
        }
    }
}
