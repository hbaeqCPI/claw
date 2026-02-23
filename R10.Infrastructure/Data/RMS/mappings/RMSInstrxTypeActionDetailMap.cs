using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSInstrxTypeActionDetailMap : IEntityTypeConfiguration<RMSInstrxTypeActionDetail>
    {
        public void Configure(EntityTypeBuilder<RMSInstrxTypeActionDetail> builder)
        {
            builder.ToTable("tblRMSInstrxTypeActionDetail");
            builder.HasKey(i => i.InstrxTypeActionDetailId);
            builder.HasIndex(i => new { i.InstrxTypeActionId, i.InstructionType}).IsUnique();
        }
    }
}
