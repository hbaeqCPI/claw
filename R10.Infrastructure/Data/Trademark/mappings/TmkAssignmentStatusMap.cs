using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkAssignmentStatusMap : IEntityTypeConfiguration<TmkAssignmentStatus>
    {
        public void Configure(EntityTypeBuilder<TmkAssignmentStatus> builder)
        {
            builder.ToTable("tblTmkAssignmentStatus");
            builder.Property(s => s.AssignmentStatusId).ValueGeneratedOnAdd();
            builder.Property(s => s.AssignmentStatusId).UseIdentityColumn();
            builder.Property(s => s.AssignmentStatusId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.AssignmentStatus).IsUnique();
            builder.HasMany(m => m.AssignmentsHistory).WithOne(o => o.TmkAssignmentStatus).HasForeignKey(h => h.AssignmentStatus).HasPrincipalKey(s => s.AssignmentStatus);


        }
    }
}
