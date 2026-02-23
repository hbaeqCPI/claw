using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSActionCloseLogErrorMap : IEntityTypeConfiguration<RMSActionCloseLogError>
    {
        public void Configure(EntityTypeBuilder<RMSActionCloseLogError> builder)
        {
            builder.ToTable("tblRMSActionCloseLogError");
            builder.HasKey(e => e.LogErrorId);
            builder.HasOne(e => e.RMSActionCloseLog).WithMany(l => l.RMSActionCloseLogErrors).HasForeignKey(e => e.LogId).HasPrincipalKey(l => l.LogId);
        }
    }
}
