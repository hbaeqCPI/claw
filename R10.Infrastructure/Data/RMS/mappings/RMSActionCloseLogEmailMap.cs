using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSActionCloseLogEmailMap : IEntityTypeConfiguration<RMSActionCloseLogEmail>
    {
        public void Configure(EntityTypeBuilder<RMSActionCloseLogEmail> builder)
        {
            builder.ToTable("tblRMSActionCloseLogEmail");
            builder.HasKey(e => e.LogEmailId);
            builder.HasOne(e => e.RMSActionCloseLog).WithMany(l => l.RMSActionCloseLogEmails).HasForeignKey(e => e.LogId).HasPrincipalKey(l => l.LogId);
        }
    }
}
