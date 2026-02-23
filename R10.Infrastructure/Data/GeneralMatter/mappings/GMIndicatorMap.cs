using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMIndicatorMap : IEntityTypeConfiguration<GMIndicator>
    {
        public void Configure(EntityTypeBuilder<GMIndicator> builder)
        {
            builder.ToTable("tblGMIndicator");
            builder.Property(i => i.IndicatorId).ValueGeneratedOnAdd();
            builder.Property(i => i.IndicatorId).UseIdentityColumn();
            builder.Property(i => i.IndicatorId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(i => i.DueDates).WithOne(d => d.GMIndicator).HasForeignKey(d => d.Indicator).HasPrincipalKey(i => i.Indicator);
        }
    }
}
