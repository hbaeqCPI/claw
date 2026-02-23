using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSDateTypeControlMap : IEntityTypeConfiguration<RSDateTypeControl>
    {
        public void Configure(EntityTypeBuilder<RSDateTypeControl> builder)
        {
            builder.ToTable("tblRSDateTypeControl");
            builder.HasKey("DateTypeId");
            builder.HasIndex(a => new { a.ReportId, a.DateTypeName }).IsUnique();
        }
    }
}

