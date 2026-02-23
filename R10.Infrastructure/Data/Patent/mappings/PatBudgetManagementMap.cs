using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatBudgetManagementMap : IEntityTypeConfiguration<PatBudgetManagement>
    {
        public void Configure(EntityTypeBuilder<PatBudgetManagement> builder)
        {
            builder.ToTable("tblPatBudgetManagement");
            builder.Property(s => s.BMId).ValueGeneratedOnAdd();
            builder.Property(m => m.BMId).UseIdentityColumn();
            builder.HasOne(b => b.PatCostType).WithMany(ct => ct.PatBudgetManagements).HasForeignKey(b => b.CostType).HasPrincipalKey(ct=>ct.CostType);
            builder.HasOne(b => b.PatCountry).WithMany(c => c.PatBudgetManagements).HasForeignKey(b => b.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
