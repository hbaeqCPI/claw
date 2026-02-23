using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSRemLogDueMap : IEntityTypeConfiguration<RMSRemLogDue>
    {
        public void Configure(EntityTypeBuilder<RMSRemLogDue> builder)
        {
            builder.ToTable("tblRMSRemLogDue");
            builder.HasKey(d => d.LogDueId);
            builder.HasOne(d => d.RemLog).WithMany(l => l.RemLogDues).HasForeignKey(d => d.RemId).HasPrincipalKey(l => l.RemId);
            builder.HasOne(d => d.DueDetail).WithMany(due => due.RMSRemLogDues).HasForeignKey(d => d.DueId).HasPrincipalKey(l => l.DDId);
        }
    }
}
