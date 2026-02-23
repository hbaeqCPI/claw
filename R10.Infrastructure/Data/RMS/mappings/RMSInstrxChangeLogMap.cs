using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSInstrxChangeLogMap : IEntityTypeConfiguration<RMSInstrxChangeLog>
    {
        public void Configure(EntityTypeBuilder<RMSInstrxChangeLog> builder)
        {
            builder.ToTable("tblRMSInstrxChangeLog");
            builder.HasKey(l => l.LogId);
            builder.HasOne(l => l.RMSDue).WithOne(d => d.RMSInstrxChangeLog).HasPrincipalKey<RMSInstrxChangeLog>(l => l.LogId).HasForeignKey<RMSDue>(d => d.ClientInstructionLogId);
        }
    }
}
