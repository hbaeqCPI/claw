using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSActionCloseLogMap : IEntityTypeConfiguration<RMSActionCloseLog>
    {
        public void Configure(EntityTypeBuilder<RMSActionCloseLog> builder)
        {
            builder.ToTable("tblRMSActionCloseLog");
            builder.HasKey(l => l.LogId);
        }
    }
}
