using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatInventorAwardCriteriaMap : IEntityTypeConfiguration<PatInventorAwardCriteria>
    {
        public void Configure(EntityTypeBuilder<PatInventorAwardCriteria> builder)
        {
            builder.ToTable("tblPatInventorAwardCriteria");
            builder.Property(s => s.AwardCriteriaId).ValueGeneratedOnAdd();
            builder.Property(m => m.AwardCriteriaId).UseIdentityColumn();
            builder.HasIndex(s => new { s.CaseType, s.Country, s.AwardTypeId }).IsUnique();
            builder.HasOne(a => a.PatInventorAwardType).WithMany(t => t.PatInventorAwardCriterias).HasForeignKey(t => t.AwardTypeId).HasPrincipalKey(t => t.AwardTypeId);
            builder.HasOne(a => a.PatCountry).WithMany(c => c.PatInventorAwardCriterias).HasForeignKey(t => t.Country).HasPrincipalKey(t => t.Country);
        }
    }
}
