using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFInstrxTypeActionMap : IEntityTypeConfiguration<FFInstrxTypeAction>
    {
        public void Configure(EntityTypeBuilder<FFInstrxTypeAction> builder)
        {
            builder.ToTable("tblFFInstrxTypeAction");
            builder.HasKey(i => i.InstrxTypeActionId);
            builder.HasIndex(i => i.ActionType).IsUnique();
            builder.HasMany(i => i.FFInstrxTypeActionDetail).WithOne(d => d.FFInstrxTypeAction).HasPrincipalKey(s => s.InstrxTypeActionId).HasForeignKey(d => d.InstrxTypeActionId);
            builder.HasMany(i => i.PatActionDues).WithOne(d => d.FFInstrxTypeAction).HasPrincipalKey(i => i.ActionType).HasForeignKey(d => d.ActionType).IsRequired(false);
        }
    }
}
