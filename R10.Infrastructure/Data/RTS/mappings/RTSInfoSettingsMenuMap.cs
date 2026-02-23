using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RTS.mappings
{
    public class RTSInfoSettingsMenuMap : IEntityTypeConfiguration<RTSInfoSettingsMenu>
    {
        public void Configure(EntityTypeBuilder<RTSInfoSettingsMenu> builder)
        {
            builder.ToTable("tblPLInfoSettingsMenu");
        }
    }
}
