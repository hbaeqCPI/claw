using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class CPiSystemSettingMap : IEntityTypeConfiguration<CPiSystemSetting>
    {
        public void Configure(EntityTypeBuilder<CPiSystemSetting> builder)
        {
            builder.ToTable("tblCPiSystemSettings");
            builder.HasKey(x => new { x.Id });
        }
    }
}
