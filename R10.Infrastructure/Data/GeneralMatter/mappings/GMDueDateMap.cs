using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMDueDateMap : IEntityTypeConfiguration<GMDueDate>
    {
        public void Configure(EntityTypeBuilder<GMDueDate> builder)
        {
            builder.ToTable("tblGMDueDate");
            builder.HasIndex(a => new { a.ActId, a.ActionDue, a.DueDate }).IsUnique();
            builder.HasOne(a => a.GMActionDue).WithMany(a => a.DueDates).HasForeignKey(a => a.ActId);
            builder.HasOne(a => a.DeDocketOutstanding).WithOne(a => a.GMDueDate).HasForeignKey<GMDueDateDeDocketOutstanding>(a => a.DDId).HasPrincipalKey<GMDueDate>(a => a.DDId);
        }
    }
}
