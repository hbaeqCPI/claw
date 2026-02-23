using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMBudgetManagementMap : IEntityTypeConfiguration<GMBudgetManagement>
    {
        public void Configure(EntityTypeBuilder<GMBudgetManagement> builder)
        {
            builder.ToTable("tblGMBudgetManagement");
            builder.Property(s => s.BMId).ValueGeneratedOnAdd();
            builder.Property(m => m.BMId).UseIdentityColumn();
            builder.HasOne(b => b.GMCostType).WithMany(ct => ct.GMBudgetManagements).HasForeignKey(b => b.CostType).HasPrincipalKey(ct => ct.CostType);
            builder.HasOne(b => b.GMCountry).WithMany(c => c.GMBudgetManagements).HasForeignKey(b => b.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
