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
    public class PatIRFRValorizationRuleMap : IEntityTypeConfiguration<PatIRFRValorizationRule>
    {
        public void Configure(EntityTypeBuilder<PatIRFRValorizationRule> builder)
        {
            builder.ToTable("tblPatIRFRValorizationRule");
            builder.Property(c => c.ValorizationRuleId).ValueGeneratedOnAdd();
            builder.Property(m => m.ValorizationRuleId).UseIdentityColumn();
            //builder.Property(m => m.PositionId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(c => c.Point).IsUnique();
        }
    }
}
