using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIRRemunerationFormulaFactorMap : IEntityTypeConfiguration<PatIRRemunerationFormulaFactor>
    {
        public void Configure(EntityTypeBuilder<PatIRRemunerationFormulaFactor> builder)
        {
            builder.ToTable("tblPatIRRemunerationFormulaFactor");
            builder.Property(c => c.FactorId).ValueGeneratedOnAdd();
            builder.Property(m => m.FactorId).UseIdentityColumn();
            builder.HasIndex(c => c.Variable).IsUnique();
            builder.HasIndex(c => c.Name).IsUnique();
        }
    }
}
