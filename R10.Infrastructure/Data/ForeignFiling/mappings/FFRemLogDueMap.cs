using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFRemLogDueMap : IEntityTypeConfiguration<FFRemLogDue>
    {
        public void Configure(EntityTypeBuilder<FFRemLogDue> builder)
        {
            builder.ToTable("tblFFRemLogDue");
            builder.HasKey(d => d.LogDueId);
            builder.HasOne(d => d.RemLog).WithMany(l => l.RemLogDues).HasForeignKey(d => d.RemId).HasPrincipalKey(l => l.RemId);
            builder.HasOne(d => d.DueDetail).WithMany(due => due.FFRemLogDues).HasForeignKey(d => d.DueId).HasPrincipalKey(l => l.DDId);
        }
    }
}
