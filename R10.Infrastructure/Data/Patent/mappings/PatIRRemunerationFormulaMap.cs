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
    public class PatIRRemunerationFormulaMap : IEntityTypeConfiguration<PatIRRemunerationFormula>
    {
        public void Configure(EntityTypeBuilder<PatIRRemunerationFormula> builder)
        {
            builder.ToTable("tblPatIRRemunerationFormula");
            builder.Property(c => c.FormulaId).ValueGeneratedOnAdd();
            builder.Property(m => m.FormulaId).UseIdentityColumn();
            builder.HasIndex(c => c.Name).IsUnique();
        }
    }
}
