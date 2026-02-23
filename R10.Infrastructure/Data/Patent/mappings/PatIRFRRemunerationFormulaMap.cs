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
    public class PatIRFRRemunerationFormulaMap : IEntityTypeConfiguration<PatIRFRRemunerationFormula>
    {
        public void Configure(EntityTypeBuilder<PatIRFRRemunerationFormula> builder)
        {
            builder.ToTable("tblPatIRFRRemunerationFormula");
            builder.Property(c => c.FormulaId).ValueGeneratedOnAdd();
            builder.Property(m => m.FormulaId).UseIdentityColumn();
            builder.HasIndex(c => c.Name).IsUnique();
        }
    }
}
