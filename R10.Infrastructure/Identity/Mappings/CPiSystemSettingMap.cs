using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
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
