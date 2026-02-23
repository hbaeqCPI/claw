using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSInstrxTypeMap : IEntityTypeConfiguration<RMSInstrxType>
    {
        public void Configure(EntityTypeBuilder<RMSInstrxType> builder)
        {
            builder.ToTable("tblRMSInstrxType");
            builder.Property(i => i.InstructionId).ValueGeneratedOnAdd();
            builder.Property(i => i.InstructionId).UseIdentityColumn();
            builder.Property(i => i.InstructionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasKey(i => i.InstructionType);
            builder.HasMany(i => i.RMSDues).WithOne(d => d.ClientInstrxType).HasForeignKey(d => d.ClientInstructionType).HasPrincipalKey(i => i.InstructionType);
            builder.HasMany(i => i.RMSInstrxTypeActionDetail).WithOne(d => d.RMSInstrxType).HasForeignKey(d => d.InstructionType).HasPrincipalKey(i => i.InstructionType);
        }
    }
}
