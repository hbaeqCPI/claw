using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateInvDeDocketOutstandingMap : IEntityTypeConfiguration<PatDueDateInvDeDocketOutstanding>
    {
        public void Configure(EntityTypeBuilder<PatDueDateInvDeDocketOutstanding> builder)
        {
            builder.ToTable("vwPatInvDeDocketOutstanding");
            //builder.HasOne(dd => dd.PatDueDate).WithOne(d => d.DeDocketOutstanding).HasForeignKey<PatDueDate>();

        }
    }
}
