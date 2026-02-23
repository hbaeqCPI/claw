using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatPriorityMap : IEntityTypeConfiguration<PatPriority>
    {
        public void Configure(EntityTypeBuilder<PatPriority> builder)
        {
            builder.ToTable("tblPatPriority");
            builder.HasIndex(pp => new { pp.PriId, pp.InvId }).IsUnique();
            builder.HasOne(pp => pp.PriorityCountry).WithMany(c => c.CountryPriorities).HasForeignKey(pp => pp.Country).HasPrincipalKey(c => c.Country);
            builder.HasOne(pp => pp.PriorityCaseType).WithMany(c => c.CaseTypePriorities).HasForeignKey(pp => pp.CaseType).HasPrincipalKey(c => c.CaseType);
            builder.HasOne(pp => pp.Invention).WithMany(i => i.Priorities).HasForeignKey(p => p.InvId).HasPrincipalKey(i => i.InvId);
        }
    }
}
