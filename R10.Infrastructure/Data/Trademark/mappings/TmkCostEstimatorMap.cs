using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCostEstimatorMap : IEntityTypeConfiguration<TmkCostEstimator>
    {
        public void Configure(EntityTypeBuilder<TmkCostEstimator> builder)
        {
            builder.ToTable("tblTmkCostEstimator");
            builder.HasIndex(c => new { c.Name }).IsUnique();
            builder.HasOne(c => c.BaseTmkTrademark).WithMany(c => c.CostEstimators).HasPrincipalKey(c => c.TmkId).HasForeignKey(d => d.TmkId);            
            builder.HasOne(c => c.TmkDueDate).WithMany(c => c.TmkCostEstimators).HasPrincipalKey(c => c.DDId).HasForeignKey(d => d.DDId);
        }
    }
}
