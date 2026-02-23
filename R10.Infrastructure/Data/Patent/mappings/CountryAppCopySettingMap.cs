using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class CountryAppCopySettingMap : IEntityTypeConfiguration<CountryApplicationCopySetting>
    {
        public void Configure(EntityTypeBuilder<CountryApplicationCopySetting> builder)
        {
            builder.ToTable("tblPatCountryApplicationCopySetting");
        }
    }

    //public class CountryAppCopySettingChildMap : IEntityTypeConfiguration<CountryApplicationCopySettingChild>
    //{
    //    public void Configure(EntityTypeBuilder<CountryApplicationCopySettingChild> builder)
    //    {
    //        builder.ToTable("tblPatCountryApplicationCopySettingChild");
    //    }
    //}
}
