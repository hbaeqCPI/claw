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
    public class PatIRFRRemunerationValuationMatrixTypeMap : IEntityTypeConfiguration<PatIRFRRemunerationValuationMatrixType>
    {
        public void Configure(EntityTypeBuilder<PatIRFRRemunerationValuationMatrixType> builder)
        {
            builder.ToTable("tblPatIRFRRemunerationValuationMatrixType");
            builder.Property(c => c.MatrixTypeId).ValueGeneratedOnAdd();
            builder.Property(m => m.MatrixTypeId).UseIdentityColumn();
            builder.HasIndex(c => c.MatrixType).IsUnique();
        }
    }
}