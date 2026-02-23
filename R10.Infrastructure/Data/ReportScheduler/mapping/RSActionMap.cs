using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSActionMap : IEntityTypeConfiguration<RSAction>
    {
        public void Configure(EntityTypeBuilder<RSAction> builder)
        {
            builder.ToTable("tblRSAction");
            builder.HasKey("ActionId");
        }
    }
}
