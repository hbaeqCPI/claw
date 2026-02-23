using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSInstrxCPiLogMap : IEntityTypeConfiguration<AMSInstrxCPiLog>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxCPiLog> builder)
        {
            builder.ToTable("tblAMSInstrxCPILog");
            builder.HasKey(l => l.SendId);
        }
    }
}
