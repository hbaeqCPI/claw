using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSInstrxCPiLogEmailMap : IEntityTypeConfiguration<AMSInstrxCPiLogEmail>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxCPiLogEmail> builder)
        {
            builder.ToTable("tblAMSInstrxCPiLogEmail");
            builder.HasKey(e => e.LogEmailId);
            builder.HasOne(e => e.AMSInstrxCPiLog).WithMany(l => l.AMSInstrxCPiLogEmails).HasForeignKey(e => e.SendId).HasPrincipalKey(l => l.SendId);
        }
    }
}
