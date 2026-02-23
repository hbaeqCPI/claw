using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSActionHistoryMap : IEntityTypeConfiguration<RSActionHistory>
    {
        public void Configure(EntityTypeBuilder<RSActionHistory> builder)
        {
            builder.ToTable("tblRSActionHistory");
            builder.HasKey("ActionHistoryId");
        }
    }
}
