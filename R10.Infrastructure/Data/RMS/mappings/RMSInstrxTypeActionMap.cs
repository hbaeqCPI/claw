using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSInstrxTypeActionMap : IEntityTypeConfiguration<RMSInstrxTypeAction>
    {
        public void Configure(EntityTypeBuilder<RMSInstrxTypeAction> builder)
        {
            builder.ToTable("tblRMSInstrxTypeAction");
            builder.HasKey(i => i.InstrxTypeActionId);
            builder.HasIndex(i => i.ActionType).IsUnique();
            builder.HasMany(i => i.RMSInstrxTypeActionDetail).WithOne(d => d.RMSInstrxTypeAction).HasForeignKey(d => d.InstrxTypeActionId).HasPrincipalKey(i => i.InstrxTypeActionId);
        }
    }
}
