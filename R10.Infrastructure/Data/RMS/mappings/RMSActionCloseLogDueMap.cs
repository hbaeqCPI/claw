using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSActionCloseLogDueMap : IEntityTypeConfiguration<RMSActionCloseLogDue>
    {
        public void Configure(EntityTypeBuilder<RMSActionCloseLogDue> builder)
        {
            builder.ToTable("tblRMSActionCloseLogDue");
            builder.HasKey(d => d.LogDueId);
            builder.HasIndex(d => new { d.LogId, d.DueId }).IsUnique();
            builder.HasOne(d => d.RMSActionCloseLog).WithMany(l => l.RMSActionCloseLogDues).HasForeignKey(d => d.LogId).HasPrincipalKey(l => l.LogId);
            builder.HasOne(d => d.TmkDueDate).WithMany(due => due.RMSActionCloseLogDues).HasForeignKey(d => d.DueId).HasPrincipalKey(d => d.DDId);
            builder.HasOne(d => d.RMSInstrxChangeLog).WithOne(l => l.RMSActionCloseLogDue).HasPrincipalKey<RMSInstrxChangeLog>(l => l.LogId).HasForeignKey<RMSActionCloseLogDue>(d => d.ClientInstructionLogId);
            builder.HasOne(d => d.SentInstrxType).WithMany(t => t.SentInstructionTypes).HasForeignKey(d => d.SentInstructionType).HasPrincipalKey(i => i.InstructionType);
        }
    }
}
