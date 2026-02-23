using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class InventionCopySettingMap : IEntityTypeConfiguration<InventionCopySetting>
    {
        public void Configure(EntityTypeBuilder<InventionCopySetting> builder)
        {
            builder.ToTable("tblPatInventionCopySetting");
        }
    }
}
