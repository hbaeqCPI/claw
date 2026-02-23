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
    public class PatIRRemunerationValuationMatrixCriteriaMap : IEntityTypeConfiguration<PatIRRemunerationValuationMatrixCriteria>
    {
        public void Configure(EntityTypeBuilder<PatIRRemunerationValuationMatrixCriteria> builder)
        {
            builder.ToTable("tblPatIRRemunerationValuationMatrixCriteria");
            builder.Property(c => c.CriteriaId).ValueGeneratedOnAdd();
            builder.Property(m => m.CriteriaId).UseIdentityColumn();
            builder.HasIndex(c => new {c.MatrixId, c.Category}).IsUnique();
        }
    }
}