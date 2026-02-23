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
    public class PatIRFRRemunerationValuationMatrixDataMap : IEntityTypeConfiguration<PatIRFRRemunerationValuationMatrixData>
    {
        public void Configure(EntityTypeBuilder<PatIRFRRemunerationValuationMatrixData> builder)
        {
            builder.ToTable("tblPatIRFRRemunerationValuationMatrixData");
            builder.Property(c => c.DataId).ValueGeneratedOnAdd();
            builder.Property(m => m.DataId).UseIdentityColumn();
            builder.HasOne(a => a.FRRemuneration).WithMany(c => c.ValuationMatrixData).HasForeignKey(t => t.FRRemunerationId).HasPrincipalKey(t => t.FRRemunerationId);
        }
    }
}