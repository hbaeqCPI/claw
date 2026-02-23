using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSCriteriaControlMap : IEntityTypeConfiguration<RSCriteriaControl>
    {
        public void Configure(EntityTypeBuilder<RSCriteriaControl> builder)
        {
            builder.ToTable("tblRSCriteriaControl");
            builder.HasKey("CriteriaId");
            builder.HasIndex(a => new { a.ReportId, a.FieldName }).IsUnique();
        }
    }
}
