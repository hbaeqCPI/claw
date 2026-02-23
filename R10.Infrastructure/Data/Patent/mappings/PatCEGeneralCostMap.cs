using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCEGeneralCostMap : IEntityTypeConfiguration<PatCEGeneralCost>
    {
        public void Configure(EntityTypeBuilder<PatCEGeneralCost> builder)
        {
            builder.ToTable("tblPatCEGeneralCost");
            builder.HasKey("CostId");
            builder.HasIndex(c => new { c.Description }).IsUnique();            
            builder.HasOne(c => c.PatCEGeneralSetup).WithMany(c =>c.PatCEGeneralCosts).HasPrincipalKey(c => c.CEGeneralId).HasForeignKey(d => d.CEGeneralId);
            builder.HasMany(g => g.PatCEQuestionGenerals).WithOne(q => q.PatCEGeneralCost).HasForeignKey(q => q.CostId).HasPrincipalKey(g => g.CostId);
            builder.HasOne(c => c.PatCEStage).WithMany(c =>c.PatCEGeneralCosts).HasPrincipalKey(c => c.Stage).HasForeignKey(d => d.Stage);
        }
    }
}
