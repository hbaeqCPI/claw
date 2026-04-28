using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class CPiUserSettingMap : IEntityTypeConfiguration<CPiUserSetting>
    {
        public void Configure(EntityTypeBuilder<CPiUserSetting> builder)
        {
            builder.ToTable("tblCPiUserSettings");
            builder.HasKey(x => new { x.Id });
            builder.HasOne(s => s.CPiUser).WithMany(u => u.CPiUserSettings).HasForeignKey(s => s.UserId).HasPrincipalKey(u => u.Id);
        }
    }
}
