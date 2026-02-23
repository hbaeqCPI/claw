using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCEGeneralCostMap : IEntityTypeConfiguration<TmkCEGeneralCost>
    {
        public void Configure(EntityTypeBuilder<TmkCEGeneralCost> builder)
        {
            builder.ToTable("tblTmkCEGeneralCost");
            builder.HasKey("CostId");
            builder.HasIndex(c => new { c.Description }).IsUnique();            
            builder.HasOne(c => c.TmkCEGeneralSetup).WithMany(c =>c.TmkCEGeneralCosts).HasPrincipalKey(c => c.CEGeneralId).HasForeignKey(d => d.CEGeneralId);
            builder.HasMany(g => g.TmkCEQuestionGenerals).WithOne(q => q.TmkCEGeneralCost).HasForeignKey(q => q.CostId).HasPrincipalKey(g => g.CostId);
            builder.HasOne(c => c.TmkCEStage).WithMany(c =>c.TmkCEGeneralCosts).HasPrincipalKey(c => c.Stage).HasForeignKey(d => d.Stage);
        }
    }
}
