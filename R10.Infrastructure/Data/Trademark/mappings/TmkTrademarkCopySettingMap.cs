using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkTrademarkCopySettingMap : IEntityTypeConfiguration<TmkTrademarkCopySetting>
    {
        public void Configure(EntityTypeBuilder<TmkTrademarkCopySetting> builder)
        {
            builder.ToTable("tblTmkTrademarkCopySetting");
        }
    }

   
}
