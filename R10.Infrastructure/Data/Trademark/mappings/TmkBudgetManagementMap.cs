using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkBudgetManagementMap : IEntityTypeConfiguration<TmkBudgetManagement>
    {
        public void Configure(EntityTypeBuilder<TmkBudgetManagement> builder)
        {
            builder.ToTable("tblTmkBudgetManagement");
            builder.Property(s => s.BMId).ValueGeneratedOnAdd();
            builder.Property(m => m.BMId).UseIdentityColumn();
            builder.HasOne(b => b.TmkCostType).WithMany(ct => ct.TmkBudgetManagements).HasForeignKey(b => b.CostType).HasPrincipalKey(ct => ct.CostType);
            builder.HasOne(b => b.TmkCountry).WithMany(c => c.TmkBudgetManagements).HasForeignKey(b => b.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
