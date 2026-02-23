using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSIndicatorMap : IEntityTypeConfiguration<DMSIndicator>
    {
        public void Configure(EntityTypeBuilder<DMSIndicator> builder)
        {
            builder.ToTable("tblDMSIndicator");                        
            builder.Property(a => a.IndicatorId).ValueGeneratedOnAdd();
            builder.Property(i => i.IndicatorId).UseIdentityColumn();
            builder.Property(i => i.IndicatorId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(a => a.Indicator).IsUnique();
            builder.HasMany(s => s.DMSActionParameters).WithOne(d => d.DMSIndicator).HasForeignKey(s => s.Indicator).HasPrincipalKey(d => d.Indicator);
            builder.HasMany(s => s.DMSDueDates).WithOne(d => d.DMSIndicator).HasForeignKey(s => s.Indicator).HasPrincipalKey(d => d.Indicator);
        }
    }
}
