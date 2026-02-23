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
    public class RMSRemLogEmailMap : IEntityTypeConfiguration<RemLogEmail<TmkDueDate, RMSRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLogEmail<TmkDueDate, RMSRemLogDue>> builder)
        {
            builder.ToTable("tblRMSRemLogEmail");
            builder.HasKey(e => e.LogEmailId);
            builder.HasOne(e => e.RemLog).WithMany(l => l.RemLogEmails).HasForeignKey(e => e.RemId).HasPrincipalKey(l => l.RemId);
        }
    }
}
