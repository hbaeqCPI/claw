using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSMainMap : IEntityTypeConfiguration<RSMain>
    {
        public void Configure(EntityTypeBuilder<RSMain> builder)
        {
            builder.ToTable("tblRSMain");
            builder.HasKey("TaskId");
            builder.HasIndex(a => new { a.Name }).IsUnique();
            builder.HasMany(m => m.RSActions).WithOne(a => a.RSMain).HasForeignKey(a => a.TaskId).HasPrincipalKey(m=>m.TaskId);
            builder.HasMany(m => m.RSCriterias).WithOne(a => a.RSMain).HasForeignKey(a => a.TaskId).HasPrincipalKey(m => m.TaskId);
            builder.HasMany(m => m.RSPrintOptions).WithOne(a => a.RSMain).HasForeignKey(a => a.TaskId).HasPrincipalKey(m => m.TaskId);
        }
    }
}
