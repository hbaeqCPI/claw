using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSDueDateMap : IEntityTypeConfiguration<DMSDueDate>
    {
        public void Configure(EntityTypeBuilder<DMSDueDate> builder)
        {
            builder.ToTable("tblDMSDueDate");
            builder.HasIndex(a => new { a.ActId, a.ActionDue, a.DueDate }).IsUnique();
            builder.HasOne(a => a.DMSActionDue).WithMany(a => a.DueDates).HasForeignKey(a => a.ActId); 
        }
    }
}
