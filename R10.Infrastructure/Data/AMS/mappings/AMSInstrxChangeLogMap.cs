using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSInstrxChangeLogMap : IEntityTypeConfiguration<AMSInstrxChangeLog>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxChangeLog> builder)
        {
            builder.ToTable("tblAMSInstrxChangeLog");
            builder.HasKey(l => l.LogID);
            builder.HasOne(l => l.AMSDue).WithMany(d => d.AMSInstrxChangeLogs).HasPrincipalKey(d => d.DueID).HasForeignKey(l => l.DueID);
            builder.HasOne(l => l.AMSInstrxCPiLogDetail).WithOne(d => d.AMSInstrxChangeLog).HasPrincipalKey<AMSInstrxChangeLog>(l => l.LogID).HasForeignKey<AMSInstrxCPiLogDetail>(d => d.ClientInstructionLogId);
        }
    }
}
