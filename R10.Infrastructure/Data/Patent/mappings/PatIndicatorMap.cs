using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIndicatorMap : IEntityTypeConfiguration<PatIndicator>
    {
        public void Configure(EntityTypeBuilder<PatIndicator> builder)
        {
            builder.ToTable("tblPatIndicator");
            builder.Property(i => i.IndicatorId).ValueGeneratedOnAdd();
            builder.Property(i => i.IndicatorId).UseIdentityColumn();
            builder.Property(i => i.IndicatorId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(i => i.Indicator).IsUnique();

            // builder.HasMany(i => i.ActionParameters) // Removed: ActionParameters no longer exists
            //     .WithOne()
            //     .HasForeignKey(p => p.Indicator)
            //     .HasPrincipalKey(i => i.Indicator);
        }
    }
}
