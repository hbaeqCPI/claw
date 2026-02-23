using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFActionCloseLogMap : IEntityTypeConfiguration<FFActionCloseLog>
    {
        public void Configure(EntityTypeBuilder<FFActionCloseLog> builder)
        {
            builder.ToTable("tblFFActionCloseLog");
            builder.HasKey(l => l.LogId);
        }
    }
}
