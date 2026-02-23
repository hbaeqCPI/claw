using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterCountryMap : IEntityTypeConfiguration<GMMatterCountry>
    {
        public void Configure(EntityTypeBuilder<GMMatterCountry> builder)
        {
            builder.ToTable("tblGMMatterCountry");
            builder.HasKey(gc => gc.CtryID);
            builder.HasIndex(gc => new { gc.MatId, gc.CtryID }).IsUnique();
            builder.HasOne(gc => gc.GMMatter).WithMany(gm => gm.Countries).HasForeignKey(gc => gc.MatId);
            builder.HasOne(gc => gc.GMCountry).WithMany(c => c.GMMatterCountries).HasForeignKey(gc => gc.Country);
        }
    }
}
