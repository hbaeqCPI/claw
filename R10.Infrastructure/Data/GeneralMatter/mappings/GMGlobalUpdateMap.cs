using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMGlobalUpdateLogMap : IEntityTypeConfiguration<GMGlobalUpdateLog>
    {
        public void Configure(EntityTypeBuilder<GMGlobalUpdateLog> builder)
        {
            builder.ToTable("tblGMGlobalUpdateLog");
        }

    }
}
