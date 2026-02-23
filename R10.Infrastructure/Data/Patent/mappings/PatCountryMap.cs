using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryMap : IEntityTypeConfiguration<PatCountry>
    {
        public void Configure(EntityTypeBuilder<PatCountry> builder)
        {
            builder.ToTable("tblPatCountry");
            builder.Property(c => c.CountryID).ValueGeneratedOnAdd();
            builder.Property(c => c.CountryID).UseIdentityColumn();
            builder.Property(c => c.CountryID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(c => c.CountryContactPersons).WithOne(cp => cp.AddressCountry);
            builder.HasMany(c => c.CountryApplications).WithOne(app => app.PatCountry).HasForeignKey(app => app.Country).HasPrincipalKey(c => c.Country);
            builder.HasMany(c => c.PatActionsDue).WithOne(app => app.PatCountry).HasForeignKey(app => app.Country).HasPrincipalKey(c => c.Country);
            builder.HasMany(c => c.PatCountryAreas).WithOne(ca => ca.AreaCountry).HasPrincipalKey(c => c.Country).HasForeignKey(ca=>ca.Country);
            builder.HasMany(c => c.ClientDesignatedCountries).WithOne(cd => cd.PatCountry).HasPrincipalKey(c => c.Country).HasForeignKey(cd => cd.DesCtry).IsRequired(false);
            builder.HasMany(c => c.PatTaxBases).WithOne(tb => tb.PatCountry).HasForeignKey(tb => tb.Country).HasPrincipalKey(c => c.Country);
        }
    }
}
