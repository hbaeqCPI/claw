using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkIndicatorMap : IEntityTypeConfiguration<TmkIndicator>
    {
        public void Configure(EntityTypeBuilder<TmkIndicator> builder)
        {
            builder.ToTable("tblTmkIndicator");
            builder.Property(i => i.IndicatorId).ValueGeneratedOnAdd();
            builder.Property(i => i.IndicatorId).UseIdentityColumn();
            builder.Property(i => i.IndicatorId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(i => i.Indicator).IsUnique();
        }
    }
}
