using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCostEstimatorMap : IEntityTypeConfiguration<PatCostEstimator>
    {
        public void Configure(EntityTypeBuilder<PatCostEstimator> builder)
        {
            builder.ToTable("tblPatCostEstimator");
            builder.HasIndex(c => new { c.Name }).IsUnique();
            builder.HasOne(c => c.BaseCountryApplication).WithMany(c => c.CostEstimators).HasPrincipalKey(c => c.AppId).HasForeignKey(d => d.AppId);
            builder.HasOne(c => c.BaseInvention).WithMany(c => c.CostEstimators).HasPrincipalKey(c => c.InvId).HasForeignKey(d => d.InvId);
            builder.HasOne(c => c.PatDueDate).WithMany(c => c.PatCostEstimators).HasPrincipalKey(c => c.DDId).HasForeignKey(d => d.DDId);
        }
    }
}
