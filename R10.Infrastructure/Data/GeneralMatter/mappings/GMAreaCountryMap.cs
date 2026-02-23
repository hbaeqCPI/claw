using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMAreaCountryMap : IEntityTypeConfiguration<GMAreaCountry>
    {
        public void Configure(EntityTypeBuilder<GMAreaCountry> builder)
        {
            builder.ToTable("tblGMAreaCountry");
            builder.HasIndex(ac => new { ac.AreaID, ac.Country }).IsUnique();
            builder.HasOne(ac => ac.GMCountry).WithMany(gc => gc.GMAreaCountries).HasForeignKey(gc => gc.Country);
            builder.HasOne(ac => ac.GMArea).WithMany(gc => gc.GMAreaCountries).HasForeignKey(gc => gc.AreaID);
        }
    }
}
