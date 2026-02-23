using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.RMS;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.RMS.mappings
{
    public class RMSRemLogErrorMap : IEntityTypeConfiguration<RemLogError<TmkDueDate, RMSRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLogError<TmkDueDate, RMSRemLogDue>> builder)
        {
            builder.ToTable("tblRMSRemLogError");
            builder.HasKey(e => e.LogErrorId);
            builder.HasOne(e => e.RemLog).WithMany(l => l.RemLogErrors).HasForeignKey(e => e.RemId).HasPrincipalKey(l => l.RemId);
        }
    }
}
