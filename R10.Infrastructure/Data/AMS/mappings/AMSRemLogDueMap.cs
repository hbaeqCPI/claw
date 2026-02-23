using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSRemLogDueMap : IEntityTypeConfiguration<AMSRemLogDue>
    {
        public void Configure(EntityTypeBuilder<AMSRemLogDue> builder)
        {
            builder.ToTable("tblAMSRemLogDue");
            builder.HasKey(d => d.LogDueId);
            builder.HasOne(d => d.RemLog).WithMany(l => l.RemLogDues).HasForeignKey(d => d.RemId).HasPrincipalKey(l => l.RemId);
            builder.HasOne(d => d.DueDetail).WithMany(due => due.AMSRemLogDues).HasForeignKey(d => d.DueId).HasPrincipalKey(l => l.DueID);
        }
    }
}
