using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiSettingMap : IEntityTypeConfiguration<CPiSetting>
    {
        public void Configure(EntityTypeBuilder<CPiSetting> builder)
        {
            builder.ToTable("tblCPiSettings");
            builder.HasKey(x => new { x.Id });
            builder.HasAlternateKey(x => new { x.Name });
            builder.HasMany<CPiUserSetting>().WithOne(s => s.CPiSetting).HasForeignKey(x => x.SettingId).HasPrincipalKey(x => x.Id).IsRequired(true);
            builder.HasMany<CPiSystemSetting>().WithOne(s => s.CPiSetting).HasForeignKey(x => x.SettingId).HasPrincipalKey(x => x.Id).IsRequired(true);
        }
    }
}
