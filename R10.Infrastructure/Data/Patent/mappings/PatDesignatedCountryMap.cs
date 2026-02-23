using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesignatedCountryMap : IEntityTypeConfiguration<PatDesignatedCountry>
    {
        public void Configure(EntityTypeBuilder<PatDesignatedCountry> builder)
        {
            builder.ToTable("tblPatDesignatedCountry");
            builder.HasOne(h => h.CountryApplication).WithMany(c=>c.DesignatedCountries).HasPrincipalKey(c => c.AppId)
                .HasForeignKey(h => h.AppId);
            builder.HasOne(h => h.Country).WithMany(c => c.PatDesignatedCountries).HasPrincipalKey(c => c.Country)
                .HasForeignKey(h => h.DesCountry);

        }
    }
}
