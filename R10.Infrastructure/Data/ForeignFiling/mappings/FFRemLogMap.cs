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
    public class FFRemLogMap : IEntityTypeConfiguration<RemLog<PatDueDate, FFRemLogDue>>
    {
        public void Configure(EntityTypeBuilder<RemLog<PatDueDate, FFRemLogDue>> builder)
        {
            builder.ToTable("tblFFRemLog");
            builder.HasKey(r => r.RemId);
        }
    }
}
