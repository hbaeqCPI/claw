using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCEAnnuitySetupMap : IEntityTypeConfiguration<PatCEAnnuitySetup>
    {
        public void Configure(EntityTypeBuilder<PatCEAnnuitySetup> builder)
        {
            builder.ToTable("tblPatCEAnnuitySetup");
            builder.HasIndex(c => new { c.Country, c.CaseType, c.EntityStatus }).IsUnique();            
            builder.HasOne(c => c.PatCountry).WithMany(c =>c.PatCEAnnuitySetups).HasPrincipalKey(c => c.Country).HasForeignKey(d => d.Country);
            builder.HasOne(c => c.PatCurrencyType).WithMany(c => c.CurrencyPatCEAnnuitySetups).HasPrincipalKey(c => c.CurrencyTypeCode).HasForeignKey(d => d.CurrencyType);
        }
    }
}
