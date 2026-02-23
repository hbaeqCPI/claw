using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostTypeMap : IEntityTypeConfiguration<TmkCostType>
    {
        public void Configure(EntityTypeBuilder<TmkCostType> builder)
        {
            builder.ToTable("tblTmkCostType");
            builder.Property(c => c.CostTypeId).ValueGeneratedOnAdd();
            builder.Property(c => c.CostTypeId).UseIdentityColumn();
            builder.Property(c => c.CostTypeId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.Property(c => c.DefaultCost).HasColumnType("Money");
        }
    }
}
