using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFInstrxChangeLogMap : IEntityTypeConfiguration<FFInstrxChangeLog>
    {
        public void Configure(EntityTypeBuilder<FFInstrxChangeLog> builder)
        {
            builder.ToTable("tblFFInstrxChangeLog");
            builder.HasKey(l => l.LogId);
            builder.HasOne(l => l.FFDue).WithOne(d => d.InstrxChangeLog).HasPrincipalKey<FFInstrxChangeLog>(l => l.LogId).HasForeignKey<FFDue>(d => d.ClientInstructionLogId);
        }
    }
}
