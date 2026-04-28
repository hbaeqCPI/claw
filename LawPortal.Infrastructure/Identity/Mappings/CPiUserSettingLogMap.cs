using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
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
