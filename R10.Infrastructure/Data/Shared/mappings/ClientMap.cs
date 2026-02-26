using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class ClientMap:IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder ) {

            builder.ToTable("tblClient");
            builder.Property(c => c.ClientCode).HasColumnName("Client");
            builder.HasIndex(c => c.ClientCode).IsUnique();
            builder.HasOne(c => c.AddressCountry).WithMany(pc => pc.CountryClients).HasForeignKey(c => c.Country).HasPrincipalKey(pc => pc.Country);
            builder.HasOne(c => c.POAddressCountry).WithMany(pc => pc.POCountryClients).HasForeignKey(c => c.POCountry).HasPrincipalKey(pc => pc.Country);
            builder.HasMany(c => c.ClientInventions).WithOne(i => i.Client);
            // Removed during deep clean
            // builder.HasMany(c => c.ClientDisclosures).WithOne(d => d.Client);
            // builder.HasMany(c => c.ClientDMSAgendas).WithOne(d => d.Client);
            builder.HasOne(o => o.ClientLanguage).WithMany(m => m.LanguageClients).HasForeignKey(f => f.Language).HasPrincipalKey(k => k.LanguageName);
        }
    }
}
