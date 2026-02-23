using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcClearanceStatusHistoryMap : IEntityTypeConfiguration<TmcClearanceStatusHistory>
    {
        public void Configure(EntityTypeBuilder<TmcClearanceStatusHistory> builder)
        {
            builder.ToTable("tblTmcClearanceStatusHistory");
            builder.HasIndex(h => h.LogID).IsUnique();
            builder.HasOne(h => h.Clearance).WithMany(d => d.TmcClearanceStatusesHistory).HasForeignKey(r => r.TmcId).HasPrincipalKey(d => d.TmcId);
        }
    }
}
