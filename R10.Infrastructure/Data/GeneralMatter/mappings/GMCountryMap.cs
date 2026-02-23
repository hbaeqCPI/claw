using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMCountryMap : IEntityTypeConfiguration<GMCountry>
    {
        public void Configure(EntityTypeBuilder<GMCountry> builder)
        {
            builder.ToTable("tblGMCountry");
            builder.Property(c => c.CountryID).ValueGeneratedOnAdd();
            builder.Property(c => c.CountryID).UseIdentityColumn();
            builder.Property(c => c.CountryID).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasMany(c => c.GMMatterCountries).WithOne(gm => gm.GMCountry);
            builder.HasMany(c => c.GMAreaCountries).WithOne(ac => ac.GMCountry).HasPrincipalKey(c => c.Country).HasForeignKey(ac => ac.Country);
        }
    }
}
