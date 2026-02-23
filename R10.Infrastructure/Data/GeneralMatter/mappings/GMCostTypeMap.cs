using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMCostTypeMap : IEntityTypeConfiguration<GMCostType>
    {
        public void Configure(EntityTypeBuilder<GMCostType> builder)
        {
            builder.ToTable("tblGMCostType");
            builder.Property(c => c.CostTypeID).ValueGeneratedOnAdd();
            builder.Property(c => c.CostTypeID).UseIdentityColumn();
            builder.Property(c => c.CostTypeID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.Property(c => c.DefaultCost).HasColumnType("Money");
        }
    }
}
