using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostTypeMap : IEntityTypeConfiguration<PatCostType>
    {
        public void Configure(EntityTypeBuilder<PatCostType> builder)
        {
            builder.ToTable("tblPatCostType");
            builder.Property(c => c.CostTypeID).ValueGeneratedOnAdd();
            builder.Property(m => m.CostTypeID).UseIdentityColumn();
            builder.Property(m => m.CostTypeID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(c => c.CostType).IsUnique();
            builder.Property(c => c.DefaultCost).HasColumnType("Money");
        }
    }
}
