using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFInstrxTypeActionDetailMap : IEntityTypeConfiguration<FFInstrxTypeActionDetail>
    {
        public void Configure(EntityTypeBuilder<FFInstrxTypeActionDetail> builder)
        {
            builder.ToTable("tblFFInstrxTypeActionDetail");
            builder.HasKey(i => i.InstrxTypeActionDetailId);
            builder.HasIndex(i => new { i.InstrxTypeActionId, i.InstructionType }).IsUnique();
        }
    }
}
