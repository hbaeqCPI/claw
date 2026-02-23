using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatGlobalUpdateLogMap : IEntityTypeConfiguration<PatGlobalUpdateLog>
    {
        public void Configure(EntityTypeBuilder<PatGlobalUpdateLog> builder)
        {
            builder.ToTable("tblPatGlobalUpdateLog");
        }

    }
}
