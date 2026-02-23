using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSInstrxCPiLogDetailMap : IEntityTypeConfiguration<AMSInstrxCPiLogDetail>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxCPiLogDetail> builder)
        {
            builder.ToTable("tblAMSInstrxCPILogDtl");
            builder.HasKey(d => d.SendDetailId);
            builder.HasIndex(d => new { d.SendId, d.DueId }).IsUnique();
            builder.HasOne(d => d.AMSInstrxCPiLog).WithMany(l => l.AMSInstrxCPiLogDetails).HasForeignKey(d => d.SendId).HasPrincipalKey(l => l.SendId);
            builder.HasOne(d => d.AMSDue).WithMany(due => due.AMSInstrxCPiLogDetails).HasForeignKey(d => d.DueId).HasPrincipalKey(due => due.DueID);
        }
    }
}
