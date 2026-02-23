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
    public class PatIRFRRemunerationValuationMatrixMap : IEntityTypeConfiguration<PatIRFRRemunerationValuationMatrix>
    {
        public void Configure(EntityTypeBuilder<PatIRFRRemunerationValuationMatrix> builder)
        {
            builder.ToTable("tblPatIRFRRemunerationValuationMatrix");
            builder.Property(c => c.MatrixId).ValueGeneratedOnAdd();
            builder.Property(m => m.MatrixId).UseIdentityColumn();
            builder.HasIndex(c => c.Name).IsUnique();
            builder.HasIndex(c => c.Variable).IsUnique();
            builder.HasMany(a => a.Criterias).WithOne(c => c.ValuationMatrix).HasForeignKey(t => t.MatrixId);
            builder.HasOne(a => a.IRFRMatrixType).WithMany(c => c.Matrixes).HasForeignKey(t => t.MatrixType).HasPrincipalKey(t => t.MatrixType);
        }
    }
}