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
    public class PatIRRemunerationValuationMatrixDataMap : IEntityTypeConfiguration<PatIRRemunerationValuationMatrixData>
    {
        public void Configure(EntityTypeBuilder<PatIRRemunerationValuationMatrixData> builder)
        {
            builder.ToTable("tblPatIRRemunerationValuationMatrixData");
            builder.Property(c => c.DataId).ValueGeneratedOnAdd();
            builder.Property(m => m.DataId).UseIdentityColumn();
            builder.HasOne(a => a.Remuneration).WithMany(c => c.ValuationMatrixData).HasForeignKey(t => t.RemunerationId).HasPrincipalKey(t => t.RemunerationId);
        }
    }
}