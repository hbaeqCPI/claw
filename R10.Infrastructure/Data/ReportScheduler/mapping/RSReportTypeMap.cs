using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSReportTypeMap : IEntityTypeConfiguration<RSReportType>
    {
        public void Configure(EntityTypeBuilder<RSReportType> builder)
        {
            builder.ToTable("tblRSReportType");
            builder.HasKey("ReportId");
            builder.HasIndex(a => new { a.ReportName }).IsUnique();
            builder.HasMany(m => m.RSMains).WithOne(a => a.RSReportType).HasForeignKey(a => a.ReportId).HasPrincipalKey(m => m.ReportId);
            builder.HasMany(m => m.RSCriteriaControls).WithOne(a => a.RSReportType).HasForeignKey(a => a.ReportId).HasPrincipalKey(m => m.ReportId);
        }
    }
}
