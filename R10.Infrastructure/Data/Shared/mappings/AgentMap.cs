using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class AgentMap : IEntityTypeConfiguration<Agent>
    {
        public void Configure(EntityTypeBuilder<Agent> builder)
        {

            builder.ToTable("tblAgent");
            builder.Property(a => a.AgentCode).HasColumnName("Agent");
            builder.HasIndex(a => a.AgentCode).IsUnique();
            builder.HasOne(a => a.AddressCountry).WithMany(pc => pc.CountryAgents).HasForeignKey(a => a.Country).HasPrincipalKey(pc => pc.Country);
            builder.HasOne(a => a.POAddressCountry).WithMany(pc => pc.POCountryAgents).HasForeignKey(a => a.POCountry).HasPrincipalKey(pc => pc.Country);
            builder.HasMany(a => a.AgentCountryApplications).WithOne(c => c.Agent);
            builder.HasOne(o => o.AgentLanguage).WithMany(m => m.LanguageAgents).HasForeignKey(f => f.Language).HasPrincipalKey(k => k.LanguageName);
            
            builder.HasMany(a => a.TaxAgentCountryApplications).WithOne(c => c.TaxAgent);
            builder.HasMany(a => a.LegalRepresentativeCountryApplications).WithOne(c => c.LegalRepresentative);

            
    }
    }
}
