using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSInstrxTypeMap : IEntityTypeConfiguration<AMSInstrxType>
    {
        public void Configure(EntityTypeBuilder<AMSInstrxType> builder)
        {
            builder.ToTable("tblAMSInstrxType");
            builder.HasKey(i => i.InstructionId);
            builder.HasIndex(i => i.InstructionType).IsUnique();
            builder.HasMany(i => i.ClientInstructionTypes).WithOne(d => d.ClientInstrxType).HasForeignKey(d => d.ClientInstructionType).HasPrincipalKey(i => i.InstructionType);
            builder.HasMany(i => i.CPIInstructionTypes).WithOne(d => d.CPIInstrxType).HasForeignKey(d => d.CPIInstructionType).HasPrincipalKey(i => i.InstructionType);
            builder.HasMany(i => i.SentInstructionTypes).WithOne(d => d.SentInstrxType).HasForeignKey(d => d.SentInstructionType).HasPrincipalKey(i => i.InstructionType);
            builder.HasMany(i => i.TriggerInstructionTypes).WithOne(d => d.TriggerInstrxType).HasForeignKey(d => d.TriggerInstructionType).HasPrincipalKey(i => i.InstructionType);
            builder.HasMany(i => i.DecisionMgtInstructionTypes).WithOne(d => d.AMSInstrxType).HasForeignKey(d => d.ClientInstructionType).HasPrincipalKey(i => i.InstructionType);
        }
    }
}
