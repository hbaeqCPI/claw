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
    public class FFRemLogErrorMap : IEntityTypeConfiguration<RemLogError<PatDueDate, FFRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLogError<PatDueDate, FFRemLogDue>> builder)
        {
            builder.ToTable("tblFFRemLogError");
            builder.HasKey(e => e.LogErrorId);
            builder.HasOne(e => e.RemLog).WithMany(l => l.RemLogErrors).HasForeignKey(e => e.RemId).HasPrincipalKey(l => l.RemId);
        }
    }
}
