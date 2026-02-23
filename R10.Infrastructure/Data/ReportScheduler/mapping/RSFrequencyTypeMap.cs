using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ReportScheduler;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ReportScheduler.mappings
{
    public class RSFrequencyTypeMap : IEntityTypeConfiguration<RSFrequencyType>
    {
        public void Configure(EntityTypeBuilder<RSFrequencyType> builder)
        {
            builder.ToTable("tblRSFrequencyType");
            builder.HasKey("FreqTypeId");
            //builder.HasIndex(a => new { a.Frequency }).IsUnique();
        }
    }
}
