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
    public class PatAssignmentStatusMap : IEntityTypeConfiguration<PatAssignmentStatus>
    {
        public void Configure(EntityTypeBuilder<PatAssignmentStatus> builder)
        {
            builder.ToTable("tblPatAssignmentStatus");
            builder.Property(s => s.AssignmentStatusID).ValueGeneratedOnAdd();
            builder.Property(m => m.AssignmentStatusID).UseIdentityColumn();
            builder.Property(m => m.AssignmentStatusID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.AssignmentStatus).IsUnique();
            builder.HasMany(m => m.AssignmentsHistory).WithOne(o => o.PatAssignmentStatus).HasForeignKey(h => h.AssignmentStatus).HasPrincipalKey(s => s.AssignmentStatus);
        }
    }
}
