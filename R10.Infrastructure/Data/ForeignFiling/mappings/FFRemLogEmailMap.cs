using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.ForeignFiling.mappings
{
    public class FFRemLogEmailMap : IEntityTypeConfiguration<RemLogEmail<PatDueDate, FFRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLogEmail<PatDueDate, FFRemLogDue>> builder)
        {
            builder.ToTable("tblFFRemLogEmail");
            builder.HasKey(e => e.LogEmailId);
            builder.HasOne(e => e.RemLog).WithMany(l => l.RemLogEmails).HasForeignKey(e => e.RemId).HasPrincipalKey(l => l.RemId);
        }
    }
}
