using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiUserSettingLogMap : IEntityTypeConfiguration<CPiUserSettingLog>
    {
        public void Configure(EntityTypeBuilder<CPiUserSettingLog> builder)
        {
            builder.ToTable("tblCPiUserSettingsLog");
            builder.HasKey(x => new { x.LogId });
        }
    }
}
