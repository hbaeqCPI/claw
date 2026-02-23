using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMOtherPartyMap : IEntityTypeConfiguration<GMOtherParty>
    {
        public void Configure(EntityTypeBuilder<GMOtherParty> builder)
        {
            builder.ToTable("tblGMOtherParty");
            builder.Property(e => e.OtherPartyID).ValueGeneratedOnAdd();
            builder.Property(e => e.OtherPartyID).UseIdentityColumn();
            builder.Property(e => e.OtherPartyID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            builder.HasOne(o => o.AddressCountry).WithMany(m => m.CountryOtherParties).HasForeignKey(f => f.Country).HasPrincipalKey(k => k.Country);
            builder.HasOne(o => o.POAddressCountry).WithMany(m => m.POCountryOtherParties).HasForeignKey(f => f.POCountry).HasPrincipalKey(k => k.Country);
            builder.HasOne(o => o.OtherPartyLanguage).WithMany(m => m.LanguageOtherParties).HasForeignKey(f => f.Language).HasPrincipalKey(k => k.LanguageName);
        }
    }
}
