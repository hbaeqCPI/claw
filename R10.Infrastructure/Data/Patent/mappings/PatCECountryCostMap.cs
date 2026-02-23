using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCECountryCostMap : IEntityTypeConfiguration<PatCECountryCost>
    {
        public void Configure(EntityTypeBuilder<PatCECountryCost> builder)
        {
            builder.ToTable("tblPatCECountryCost");
            builder.HasKey("CostId");
            builder.HasIndex(c => new { c.Description }).IsUnique();            
            builder.HasOne(c => c.PatCECountrySetup).WithMany(c =>c.PatCECountryCosts).HasPrincipalKey(c => c.CECountryId).HasForeignKey(d => d.CECountryId);
            builder.HasOne(c => c.PatCEStage).WithMany(c =>c.PatCECountryCosts).HasPrincipalKey(c => c.Stage).HasForeignKey(d => d.Stage);
        }
    }
}
