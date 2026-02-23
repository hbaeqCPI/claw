using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacClearanceStatusHistoryMap : IEntityTypeConfiguration<PacClearanceStatusHistory>
    {
        public void Configure(EntityTypeBuilder<PacClearanceStatusHistory> builder)
        {
            builder.ToTable("tblPacClearanceStatusHistory");
            builder.HasIndex(h => h.LogID).IsUnique();
            builder.HasOne(h => h.Clearance).WithMany(d => d.PacClearanceStatusesHistory).HasForeignKey(r => r.PacId).HasPrincipalKey(d => d.PacId);
        }
    }
}
