using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class OwnerMap : IEntityTypeConfiguration<Owner>
    {
        public void Configure(EntityTypeBuilder<Owner> builder)
        {

            builder.ToTable("tblOwner");
            builder.Property(o => o.OwnerCode).HasColumnName("Owner");
            builder.HasIndex(o => o.OwnerCode).IsUnique();
            builder.HasOne(o => o.AddressCountry).WithMany(pc => pc.CountryOwners).HasForeignKey(o => o.Country).HasPrincipalKey(pc => pc.Country);
            builder.HasOne(o => o.POAddressCountry).WithMany(pc => pc.POCountryOwners).HasForeignKey(o => o.POCountry).HasPrincipalKey(pc => pc.Country);
            //builder.HasMany(o => o.OwnerInventions).WithOne(i => i.Owner).HasForeignKey(i => i.OwnerID);
            builder.HasMany(o => o.OwnerDisclosures).WithOne(d => d.Owner).HasForeignKey(d => d.OwnerID);            
            //builder.HasMany(o => o.OwnerCountryApplications).WithOne(c => c.Owner);
            builder.HasMany(o => o.OwnerInvInventions).WithOne(oi => oi.Owner).HasForeignKey(pi => pi.OwnerID);
            builder.HasOne(o => o.OwnerLanguage).WithMany(m => m.LanguageOwners).HasForeignKey(f => f.Language).HasPrincipalKey(k => k.LanguageName);
        }
    }
}
