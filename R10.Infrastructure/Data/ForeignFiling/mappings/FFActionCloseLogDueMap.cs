using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFActionCloseLogDueMap : IEntityTypeConfiguration<FFActionCloseLogDue>
    {
        public void Configure(EntityTypeBuilder<FFActionCloseLogDue> builder)
        {
            builder.ToTable("tblFFActionCloseLogDue");
            builder.HasKey(d => d.LogDueId);
            builder.HasIndex(d => new { d.LogId, d.DueId }).IsUnique();
            builder.HasOne(d => d.FFActionCloseLog).WithMany(l => l.FFActionCloseLogDues).HasForeignKey(d => d.LogId).HasPrincipalKey(l => l.LogId); ;
            builder.HasOne(d => d.PatDueDate).WithMany(due => due.FFActionCloseLogDues).HasForeignKey(d => d.DueId).HasPrincipalKey(d => d.DDId);
            builder.HasOne(d => d.FFInstrxChangeLog).WithOne(l => l.FFActionCloseLogDue).HasPrincipalKey<FFInstrxChangeLog>(l => l.LogId).HasForeignKey<FFActionCloseLogDue>(d => d.ClientInstructionLogId);
            builder.HasOne(d => d.SentInstrxType).WithMany(t => t.SentInstructionTypes).HasForeignKey(d => d.SentInstructionType).HasPrincipalKey(i => i.InstructionType);
        }
    }
}
