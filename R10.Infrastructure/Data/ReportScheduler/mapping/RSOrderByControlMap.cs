using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSOrderByControlMap : IEntityTypeConfiguration<RSOrderByControl>
    {
        public void Configure(EntityTypeBuilder<RSOrderByControl> builder)
        {
            builder.ToTable("tblRSOrderByControl");
            builder.HasKey("OrderId");
            builder.HasIndex(a => new { a.ReportId, a.OrderByName }).IsUnique();
        }
    }
}
