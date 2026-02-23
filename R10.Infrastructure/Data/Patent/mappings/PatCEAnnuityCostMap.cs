using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCEAnnuityCostMap : IEntityTypeConfiguration<PatCEAnnuityCost>
    {
        public void Configure(EntityTypeBuilder<PatCEAnnuityCost> builder)
        {
            builder.ToTable("tblPatCEAnnuityCost");
            builder.HasKey("CostId");
            builder.HasIndex(c => new { c.CEAnnuityId, c.CostType, c.ActiveSwitch }).IsUnique();            
            builder.HasOne(c => c.PatCEAnnuitySetup).WithMany(c =>c.PatCEAnnuityCosts).HasPrincipalKey(c => c.CEAnnuityId).HasForeignKey(d => d.CEAnnuityId);            
        }
    }
}
