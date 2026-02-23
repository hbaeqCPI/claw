using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFInstrxTypeMap : IEntityTypeConfiguration<FFInstrxType>
    {
        public void Configure(EntityTypeBuilder<FFInstrxType> builder)
        {
            builder.ToTable("tblFFInstrxType");
            builder.Property(i => i.InstructionId).ValueGeneratedOnAdd();
            builder.Property(i => i.InstructionId).UseIdentityColumn();
            builder.Property(i => i.InstructionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasKey(i => i.InstructionType);
            builder.HasMany(i => i.ForeignFilingDues).WithOne(d => d.ClientInstrxType).HasForeignKey(d => d.ClientInstructionType).HasPrincipalKey(i => i.InstructionType);
            builder.HasMany(i => i.FFInstrxTypeActionDetail).WithOne(d => d.FFInstrxType).HasForeignKey(d => d.InstructionType).HasPrincipalKey(i => i.InstructionType);
            builder.HasMany(i => i.FFInstrxTypeActionDetail).WithOne(d => d.FFInstrxType).HasForeignKey(d => d.InstructionType).HasPrincipalKey(i => i.InstructionType);
        }
    }
}
