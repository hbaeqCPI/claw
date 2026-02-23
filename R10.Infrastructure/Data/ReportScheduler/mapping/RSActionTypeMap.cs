using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSActionTypeMap : IEntityTypeConfiguration<RSActionType>
    {
        public void Configure(EntityTypeBuilder<RSActionType> builder)
        {
            builder.ToTable("tblRSActionType");
            builder.HasKey("ActionTypeId");
            builder.HasIndex(a => new { a.Name }).IsUnique();
        }
    }
}
