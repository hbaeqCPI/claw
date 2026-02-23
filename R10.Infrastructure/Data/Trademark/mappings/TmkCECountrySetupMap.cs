using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCECountrySetupMap : IEntityTypeConfiguration<TmkCECountrySetup>
    {
        public void Configure(EntityTypeBuilder<TmkCECountrySetup> builder)
        {
            builder.ToTable("tblTmkCECountrySetup");
            builder.HasIndex(c => new { c.Country, c.CaseType }).IsUnique();            
            builder.HasOne(c => c.TmkCountry).WithMany(c =>c.TmkCECountrySetups).HasPrincipalKey(c => c.Country).HasForeignKey(d => d.Country);
            builder.HasOne(c => c.TmkCaseType).WithMany(ct => ct.CaseTypeCECountrySetups).HasPrincipalKey(c => c.CaseType).HasForeignKey(d => d.CaseType);         
            builder.HasOne(c => c.TmkCurrencyType).WithMany(c=> c.CurrencyTmkCECountrySetups).HasPrincipalKey(c => c.CurrencyTypeCode).HasForeignKey(d => d.CurrencyType);
        }
    }
}
