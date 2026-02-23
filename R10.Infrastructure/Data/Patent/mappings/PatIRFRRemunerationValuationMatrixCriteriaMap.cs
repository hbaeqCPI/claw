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
    public class PatIRFRRemunerationValuationMatrixCriteriaMap : IEntityTypeConfiguration<PatIRFRRemunerationValuationMatrixCriteria>
    {
        public void Configure(EntityTypeBuilder<PatIRFRRemunerationValuationMatrixCriteria> builder)
        {
            builder.ToTable("tblPatIRFRRemunerationValuationMatrixCriteria");
            builder.Property(c => c.CriteriaId).ValueGeneratedOnAdd();
            builder.Property(m => m.CriteriaId).UseIdentityColumn();
            builder.HasIndex(c => new {c.MatrixId, c.Category}).IsUnique();
        }
    }
}