using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class GMDueDateDeDocketOutstandingMap : IEntityTypeConfiguration<GMDueDateDeDocketOutstanding>
    {
        public void Configure(EntityTypeBuilder<GMDueDateDeDocketOutstanding> builder)
        {
            builder.ToTable("vwGMDeDocketOutstanding");
            //builder.HasOne(dd => dd.GMDueDate).WithOne(d => d.DeDocketOutstanding).HasForeignKey<GMDueDate>();
        }
    }
}
