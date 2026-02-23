using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class RTSInfoSettingsMenuCountryMap : IEntityTypeConfiguration<RTSInfoSettingsMenuCountry>
    {
        public void Configure(EntityTypeBuilder<RTSInfoSettingsMenuCountry> builder)
        {

            builder.ToTable("tblPLInfoSettingsMenuCountry");
            builder.HasOne(m => m.InfoSettingsMenu).WithMany(s => s.CountryInfoSettings).HasForeignKey(s => s.InfoMenuId).HasPrincipalKey(m => m.InfoMenuId);

        }
    }
}
