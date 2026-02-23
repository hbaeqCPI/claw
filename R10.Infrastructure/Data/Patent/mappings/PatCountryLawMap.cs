using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryLawMap : IEntityTypeConfiguration<PatCountryLaw>
    {
        public void Configure(EntityTypeBuilder<PatCountryLaw> builder)
        {
            builder.ToTable("tblPatCountryLaw");
            builder.HasIndex(c => new { c.Country, c.CaseType }).IsUnique();
            builder.HasOne(c => c.PatCaseType).WithMany(c => c.CaseTypeCountryLaws);
            builder.HasOne(c => c.PatCountry).WithMany(c =>c.PatCountryLaws);
            builder.HasOne(c => c.Agent).WithMany(c => c.AgentPatCountryLaws).HasForeignKey(c => c.DefaultAgent);
            builder.HasMany(c => c.PatCountryDues).WithOne(d => d.PatCountryLaw)
                .HasPrincipalKey(c => c.CountryLawID).HasForeignKey(d => d.CountryLawID);
            builder.HasOne(c => c.PatCaseType).WithMany(ct => ct.CaseTypeCountryLaws)
             .HasPrincipalKey(c => c.CaseType).HasForeignKey(d => d.CaseType);
            builder.HasMany(c => c.CountryApplications).WithOne(d => d.PatCountryLaw)
                   .HasPrincipalKey(c => new {c.Country,c.CaseType}).HasForeignKey(d => new { d.Country, d.CaseType });
            builder.HasOne(c => c.PatCountry).WithMany(c=> c.PatCountryLaws).HasPrincipalKey(c => c.Country).HasForeignKey(d => d.Country);
        }
    }
}
