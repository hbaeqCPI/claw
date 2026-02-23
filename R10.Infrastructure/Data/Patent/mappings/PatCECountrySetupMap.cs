using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCECountrySetupMap : IEntityTypeConfiguration<PatCECountrySetup>
    {
        public void Configure(EntityTypeBuilder<PatCECountrySetup> builder)
        {
            builder.ToTable("tblPatCECountrySetup");
            builder.HasIndex(c => new { c.Country, c.CaseType, c.EntityStatus }).IsUnique();            
            builder.HasOne(c => c.PatCountry).WithMany(c =>c.PatCECountrySetups).HasPrincipalKey(c => c.Country).HasForeignKey(d => d.Country);
            builder.HasOne(c => c.PatCaseType).WithMany(ct => ct.CaseTypeCECountrySetups).HasPrincipalKey(c => c.CaseType).HasForeignKey(d => d.CaseType);         
            builder.HasOne(c => c.PatCurrencyType).WithMany(c=> c.CurrencyPatCECountrySetups).HasPrincipalKey(c => c.CurrencyTypeCode).HasForeignKey(d => d.CurrencyType);
        }
    }
}
