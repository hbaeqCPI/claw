using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkGlobalUpdateLogMap : IEntityTypeConfiguration<TmkGlobalUpdateLog>
    {
        public void Configure(EntityTypeBuilder<TmkGlobalUpdateLog> builder)
        {
            builder.ToTable("tblTmkGlobalUpdateLog");
        }

    }
}
