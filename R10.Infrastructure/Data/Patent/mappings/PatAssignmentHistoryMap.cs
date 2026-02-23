using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatAssignmentHistoryMap : IEntityTypeConfiguration<PatAssignmentHistory>
    {
        public void Configure(EntityTypeBuilder<PatAssignmentHistory> builder)
        {
            builder.ToTable("tblPatAssignmentHistory");
            builder.HasIndex(a => new { a.AppId, a.AssignmentFrom, a.AssignmentTo, a.AssignmentDate, a.Reel, a.Frame }).IsUnique();
            builder.HasIndex(a => a.AssignmentFrom);
            builder.HasIndex(a => a.AssignmentTo);
            builder.HasOne(h => h.CountryApplication).WithMany(c => c.AssignmentsHistory).HasForeignKey(h => h.AppId).HasPrincipalKey(c=>c.AppId);
            
        }
    }
}
