using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.AMS.mappings
{
    public class AMSRemLogEmailMap : IEntityTypeConfiguration<RemLogEmail<AMSDue, AMSRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLogEmail<AMSDue, AMSRemLogDue>> builder)
        {
            builder.ToTable("tblAMSRemLogEmail");
            builder.HasKey(e => e.LogEmailId);
            builder.HasOne(e => e.RemLog).WithMany(l => l.RemLogEmails).HasForeignKey(e => e.RemId).HasPrincipalKey(l => l.RemId);
        }
    }
}
