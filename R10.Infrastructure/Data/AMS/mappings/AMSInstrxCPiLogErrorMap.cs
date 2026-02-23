using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSInstrxCPiLogErrorMap : IEntityTypeConfiguration<AMSInstrxCPiLogError>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxCPiLogError> builder)
        {
            builder.ToTable("tblAMSInstrxCPiLogError");
            builder.HasKey(e => e.LogErrorId);
            builder.HasOne(e => e.AMSInstrxCPiLog).WithMany(l => l.AMSInstrxCPiLogErrors).HasForeignKey(e => e.SendId).HasPrincipalKey(l => l.SendId);
        }
    }
}
